using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SistemaPulperia.Data;
using SistemaPulperia.Data.Inicializador;
using SistemaPulperia.Models;
using SistemaPulperia.Models.Entities; // Para ApplicationUser
using SistemaPulperia.Web.Models.Entities; // PARA NIVELACCESO
using SistemaPulperia.Services;
using Microsoft.AspNetCore.Authentication.Google; 

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURAR LA CADENA DE CONEXIÓN (Sincronizada con appsettings)
var connectionString = builder.Configuration.GetConnectionString("ConexionRemota") 
    ?? throw new InvalidOperationException("Connection string 'ConexionRemota' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. CONFIGURACIÓN DE IDENTITY CON NIVELACCESO (REPARADO)
builder.Services.AddIdentity<ApplicationUser, NivelAcceso>(options => {
    // Bloqueo de cuenta
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Se bloquea por 15 min
    options.Lockout.MaxFailedAccessAttempts = 3; // MÁXIMO 3 INTENTOS
    options.Lockout.AllowedForNewUsers = true;

    options.SignIn.RequireConfirmedAccount = false; // Cambiado a false para que puedas entrar ya
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
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "Pendiente";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "Pendiente";
    });

// 5. SERVICIOS DE LA APLICACIÓN
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout"; // Asegúrate de que apunte a tu controlador
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// 6. EJECUTAR EL INICIALIZADOR DE BASE DE DATOS (Seed)
await SeedDatabase();

// 7. CONFIGURAR EL PIPELINE (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();  

app.MapStaticAssets(); 

// 8. CONFIGURACIÓN DE RUTAS
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 9. MÉTODO LOCAL PARA LA SEMILLA
async Task SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.Initialize();
    }
}

app.Run();