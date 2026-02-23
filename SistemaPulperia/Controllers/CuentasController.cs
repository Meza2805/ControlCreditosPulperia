using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Data;
using SistemaPulperia.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace SistemaPulperia.Controllers
{
    public class CuentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CuentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AbrirCuenta(Cuenta nuevaCuenta)
        {
            // 1. Validar si ya tiene una cuenta activa
            var cuentaExistente = await _context.Cuentas
                .AnyAsync(c => c.PersonaId == nuevaCuenta.PersonaId && c.EstaActiva);

            if (cuentaExistente)
            {
                return Json(new { success = false, message = "El cliente ya posee una cuenta de crédito activa actualmente." });
            }

            if (ModelState.IsValid)
            {
                nuevaCuenta.FechaApertura = DateTime.Now;
                nuevaCuenta.SaldoActual = 0; // Siempre inicia en cero
                nuevaCuenta.EstaActiva = true;

                _context.Add(nuevaCuenta);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cuenta de crédito abierta exitosamente." });
            }

            return Json(new { success = false, message = "Datos inválidos en el formulario." });
        }

        [HttpGet]
        public async Task<IActionResult> ListarCuentas()
        {
            var cuentas = await _context.Cuentas
                .Include(c => c.Persona) // Unimos con Persona para traer el nombre
                .Select(c => new
                {
                    id = c.Id,
                    nombreCliente = $"{c.Persona.PrimerNombre} {c.Persona.PrimerApellido}",
                    montoMaximoCredito = c.MontoMaximoCredito,
                    saldoActual = c.SaldoActual,
                    fechaVencimiento = c.FechaVencimiento,
                    estaActiva = c.EstaActiva,
                    // Calculamos la mora en tiempo real para el cliente
                    enMora = c.EstaActiva && c.SaldoActual > 0 && c.FechaVencimiento < DateTime.Now
                })
                .ToListAsync();

            return Json(new { data = cuentas });
        }

    }
}