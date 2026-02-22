using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Data;
using SistemaPulperia.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SistemaPulperia.Controllers
{
    [Authorize]
    public class PersonasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PersonasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> ListarPersonas()
        {
            var personas = await _context.Personas
                .Select(p => new
                {
                    p.Id,
                    NombreCompleto = $"{p.PrimerNombre} {p.PrimerApellido}",
                    p.Cedula,
                    p.Telefono,
                    p.Activo,
                    p.EmailContacto
                }).ToListAsync();
            return Json(new { data = personas });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPersona(int id)
        {
            var persona = await _context.Personas.FindAsync(id);
            return Json(persona);
        }

        [HttpPost]
        public async Task<IActionResult> Guardar(Persona persona)
        {
            // ELIMINAR validaciones de navegación que causan el Error 400
            ModelState.Remove("Cuentas");
            ModelState.Remove("Cuenta"); // Por si acaso quedó rastro de la versión anterior

            if (ModelState.IsValid)
            {
                try
                {
                    if (persona.Id == 0)
                    {
                        _context.Personas.Add(persona);
                    }
                    else
                    {
                        _context.Update(persona);
                    }
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error de base de datos: " + ex.Message });
                }
            }

            // Si hay error 400, esto nos dirá qué campo exacto falló
            var errores = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return Json(new { success = false, message = "Error de validación: " + errores });
        }


        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var persona = await _context.Personas.FindAsync(id);
            if (persona == null) return Json(new { success = false });

            _context.Personas.Remove(persona);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}