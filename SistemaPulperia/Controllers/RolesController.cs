using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Data;
using SistemaPulperia.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SistemaPulperia.Web.Models.Entities;

namespace SistemaPulperia.Controllers
{
    [Authorize(Roles = "Admin")]
    
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<NivelAcceso> _roleManager;

        public RolesController(ApplicationDbContext context, RoleManager<NivelAcceso> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // Vista para asignar permisos
        public async Task<IActionResult> PermisosMenus(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return NotFound();

            ViewBag.RolNombre = rol.Name;
            ViewBag.RolId = id;

            // Traemos todos los menús y marcamos los que ya tiene el rol
            var todosLosMenus = await _context.Menus.OrderBy(m => m.Orden).ToListAsync();
            var permisosActuales = await _context.RolMenus
                .Where(rm => rm.RolId == id)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            ViewBag.PermisosActuales = permisosActuales;
           return View(todosLosMenus);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPermisos(string rolId, List<int> menusSeleccionados)
        {
            // 1. Eliminar permisos anteriores
            var antiguos = _context.RolMenus.Where(rm => rm.RolId == rolId);
            _context.RolMenus.RemoveRange(antiguos);

            // 2. Agregar nuevos
            if (menusSeleccionados != null)
            {
                foreach (var menuId in menusSeleccionados)
                {
                    _context.RolMenus.Add(new RolMenu { RolId = rolId, MenuId = menuId });
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Accesos actualizados correctamente." });
        }
    }

}