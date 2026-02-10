using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Models;
using SistemaPulperia.Models.Entities;

namespace SistemaPulperia.Data.Inicializador
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db, 
                             UserManager<ApplicationUser> userManager, 
                             RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Initialize()
        {
            // Ejecutar migraciones pendientes
            try
            {
                if ((await _db.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _db.Database.MigrateAsync();
                }
            }
            catch (Exception) { /* Manejar logs si es necesario */ }

            // Salir si los roles ya existen
            if (await _roleManager.RoleExistsAsync("Admin")) return;

            // Crear roles
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
            await _roleManager.CreateAsync(new IdentityRole("Cliente"));

            // Crear usuario administrador inicial
            var adminUser = new ApplicationUser
            {
                UserName = "lionmeza93@gmail.com",
                Email = "lionmeza93@gmail.com",
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(adminUser, "Mezapineda1993#$");

            if (result.Succeeded)
            {
                // Asignar rol de Admin al usuario creado
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}