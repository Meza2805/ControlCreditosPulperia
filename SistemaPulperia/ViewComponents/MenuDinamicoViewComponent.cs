using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Data;
using SistemaPulperia.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using SistemaPulperia.Models;

namespace SistemaPulperia.ViewComponents
{
    public class MenuDinamicoViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MenuDinamicoViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // 1. Obtener el usuario y sus roles asignados (NivelAcceso)
            var userId = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            var roles = await _userManager.GetRolesAsync(user!);

            // 2. Obtener los IDs de menús permitidos según el rol en la tabla RolMenus
            var menuIdsPermitidos = await _context.RolMenus
                .Include(rm => rm.Rol) // Esto ahora funcionará con NivelAcceso
                .Where(rm => roles.Contains(rm.Rol.Name!))
                .Select(rm => rm.MenuId)
                .Distinct()
                .ToListAsync();
            // 3. Cargar Menús principales y sus Submenús que estén activos y permitidos
            var menus = await _context.Menus
                .Include(m => m.SubMenus)
                .Where(m => m.Activo && m.PadreId == null && menuIdsPermitidos.Contains(m.Id))
                .OrderBy(m => m.Orden)
                .ToListAsync();

            // Filtrar submenús permitidos
            foreach (var menu in menus)
            {
                menu.SubMenus = menu.SubMenus
                    .Where(sm => sm.Activo && menuIdsPermitidos.Contains(sm.Id))
                    .OrderBy(sm => sm.Orden)
                    .ToList();
            }

            return View(menus);
        }
    }
}