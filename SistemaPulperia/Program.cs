using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SistemaPulperia.Data;
using SistemaPulperia.Data.Inicializador;
using SistemaPulperia.Models;
using SistemaPulperia.Services;
using Microsoft.AspNetCore.Authentication.Google; 



var builder = WebApplication.CreateBuilder(args);

// 1. Configurar la Cadena de Conexión
var connectionString = builder.Configuration.GetConnectionString("ConexionRemota") 
    ?? throw new InvalidOperationException("Connection string 'ConexionRemota' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. CONFIGURACIÓN DE IDENTITY (UNIFICADA)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = true; // REQUERIDO para tu lógica de correos
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. REGISTRO DEL EMAILSENDER (.NET 9 STYLE)
builder.Services.AddTransient<IEmailSender<ApplicationUser>, EmailSender>();

// 4. CONFIGURACIÓN DE AUTENTICACIÓN (GOOGLE)
builder.Services.AddAuthentication()
    .AddGoogle(options => {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });

// 5. SERVICIOS DE LA APLICACIÓN
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 6. EJECUTAR EL INICIALIZADOR DE BASE DE DATOS (Seed)
// Esto crea los roles y el usuario admin si no existen
await SeedDatabase();

// 7. CONFIGURAR EL PIPELINE DE SOLICITUDES HTTP (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// OJO: El orden aquí es crítico para que el Login funcione
app.UseAuthentication(); // ¿Quién eres?
app.UseAuthorization();  // ¿A qué tienes permiso?

app.MapStaticAssets(); // Optimización de .NET 9 para archivos CSS/JS

// 8. CONFIGURACIÓN DE RUTAS
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 9. MÉTODO LOCAL PARA LA SEMILLA (DbInitializer)
async Task SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.Initialize();
    }
}

app.Run();