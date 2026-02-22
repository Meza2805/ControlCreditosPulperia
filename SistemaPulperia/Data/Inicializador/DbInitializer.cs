using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Models;
using SistemaPulperia.Models.Entities;
using SistemaPulperia.Web.Models.Entities;

namespace SistemaPulperia.Data.Inicializador
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<NivelAcceso> _roleManager;

        public DbInitializer(ApplicationDbContext db,
                             UserManager<ApplicationUser> userManager,
                             RoleManager<NivelAcceso> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task Initialize()
        {
            // 1. Ejecutar migraciones pendientes en el VPS
            try
            {
                if ((await _db.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _db.Database.MigrateAsync();
                }
            }
            catch (Exception) { /* Manejar logs si es necesario */ }

            // 2. Salir si los roles ya existen (evita duplicados)
            if (await _roleManager.RoleExistsAsync("Admin")) return;

            // 3. Crear roles usando tu clase personalizada NivelAcceso
            await _roleManager.CreateAsync(new NivelAcceso
            {
                Name = "Admin",
                NormalizedName = "ADMIN",
                Activo = true,
                Descripcion = "Administrador total del sistema"
            });

            await _roleManager.CreateAsync(new NivelAcceso
            {
                Name = "Cliente",
                NormalizedName = "CLIENTE",
                Activo = true,
                Descripcion = "Acceso para clientes de la pulpería"
            });

            // 4. Crear usuario administrador inicial (Marvin Rafael Meza Pineda)
            var adminUser = new ApplicationUser
            {
                UserName = "lionmeza93@gmail.com",
                Email = "lionmeza93@gmail.com",
                EmailConfirmed = true,
                NombreCompleto = "Marvin Rafael Meza Pineda",
                FechaRegistro = DateTime.Now,
                Estado = true
            };

            // La contraseña que definiste anteriormente
            var result = await _userManager.CreateAsync(adminUser, "Mezapineda1993#$");

            if (result.Succeeded)
            {
                // 5. Asignar el rol de Admin al usuario creado
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}