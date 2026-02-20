using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Models;
using SistemaPulperia.Models.Entities;
// ESTA ES LA LÍNEA QUE TE FALTA:
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
            // ... (resto del código de inicialización)
        }
    }
}