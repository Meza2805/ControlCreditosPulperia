using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Data;
using SistemaPulperia.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SistemaPulperia.Web.Models.Entities;

namespace SistemaPulperia.Controllers
{
   
    [Authorize(Roles = "Admin,Creador")]
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

        [HttpPost]
        public async Task<IActionResult> Crear(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return Json(new { success = false, message = "El nombre es obligatorio." });

            var resultado = await _roleManager.CreateAsync(new NivelAcceso(nombre));
            if (resultado.Succeeded) return Json(new { success = true, message = "Rol creado exitosamente." });

            return Json(new { success = false, message = "Error: " + resultado.Errors.FirstOrDefault()?.Description });
        }

        [HttpPost]
        public async Task<IActionResult> Editar(string id, string nuevoNombre)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return Json(new { success = false, message = "Rol no encontrado." });

            rol.Name = nuevoNombre;
            var resultado = await _roleManager.UpdateAsync(rol);
            if (resultado.Succeeded) return Json(new { success = true, message = "Nombre actualizado correctamente." });

            return Json(new { success = false, message = "No se pudo actualizar." });
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(string id)
        {
            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null) return Json(new { success = false, message = "Rol no encontrado." });

            // Validar si el rol tiene usuarios antes de borrar (opcional pero recomendado)
            var resultado = await _roleManager.DeleteAsync(rol);
            if (resultado.Succeeded) return Json(new { success = true, message = "Rol eliminado." });

            return Json(new { success = false, message = "El rol no se puede eliminar (posiblemente tiene usuarios asignados)." });
        }
    }


}