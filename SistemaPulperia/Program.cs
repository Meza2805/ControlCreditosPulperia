using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SistemaPulperia.Data;
using SistemaPulperia.Data.Inicializador;
using SistemaPulperia.Models;


var builder = WebApplication.CreateBuilder(args);

// 1. Configurar la Cadena de Conexión
var connectionString = builder.Configuration.GetConnectionString("ConexionRemota") 
    ?? throw new InvalidOperationException("Connection string 'ConexionRemota' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IDbInitializer, DbInitializer>();
// Add services to the container.
builder.Services.AddControllersWithViews();



var app = builder.Build();

await SeedDatabase();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Método local para ejecutar la semilla
async Task SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.Initialize();
    }
}
app.Run();
