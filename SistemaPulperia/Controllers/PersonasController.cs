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
                    p.Cedula,
                    // Concatenamos todo el nombre aquí o lo enviamos separado
                    NombreCompleto = $"{p.PrimerNombre} {p.SegundoNombre} {p.PrimerApellido} {p.SegundoApellido}".Replace("  ", " ").Trim(),
                    p.Telefono,
                    p.Direccion,
                    p.Activo
                }).ToListAsync();
            return Json(new { data = personas });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPersona(int id)
        {
            var persona = await _context.Personas.FindAsync(id);
            if (persona == null) return NotFound();

            // Devolvemos el objeto tal cual
            return Json(persona);
        }

[HttpPost]
public async Task<IActionResult> Guardar(Persona persona)
{
    ModelState.Remove("Cuentas");

    if (ModelState.IsValid)
    {
        try 
        {
            // Limpiar la cédula por si acaso (aunque ya lo hacemos en el JS)
            string cedulaLimpia = persona.Cedula?.Replace("-", "") ?? "";

            // VALIDACIÓN DE DUPLICADOS
            // Buscamos si existe alguien con esa cédula que NO sea la persona actual
            bool existe = await _context.Personas
                .AnyAsync(p => p.Cedula == cedulaLimpia && p.Id != persona.Id);

            if (existe)
            {
                return Json(new { 
                    success = false, 
                    message = $"La cédula {persona.Cedula} ya pertenece a otro cliente registrado." 
                });
            }

            if (persona.Id == 0) {
                persona.Cedula = cedulaLimpia;
                _context.Personas.Add(persona);
            } else {
                persona.Cedula = cedulaLimpia;
                _context.Update(persona);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        catch (Exception ex) 
        {
            return Json(new { success = false, message = "Error de sistema: " + ex.Message });
        }
    }
    return Json(new { success = false, message = "Verifique los datos del formulario." });
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