using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Data;
using SistemaPulperia.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using SistemaPulperia.Models;

namespace SistemaPulperia.Controllers
{
    public class CuentasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CuentasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AbrirCuenta(Cuenta nuevaCuenta)
        {
            // 1. Limpiamos las validaciones de las propiedades de navegación
            // Esto evita que el ModelState falle porque 'Persona' o 'Transacciones' sean nulos
            ModelState.Remove("Persona");
            ModelState.Remove("Transacciones");

            // 2. Validar si ya tiene una cuenta activa
            var cuentaExistente = await _context.Cuentas
                .AnyAsync(c => c.PersonaId == nuevaCuenta.PersonaId && c.EstaActiva);

            if (cuentaExistente)
            {
                return Json(new { success = false, message = "El cliente ya posee una cuenta activa." });
            }

            if (ModelState.IsValid)
            {
                nuevaCuenta.FechaApertura = DateTime.Now;
                nuevaCuenta.SaldoActual = 0;
                nuevaCuenta.EstaActiva = true;

                _context.Add(nuevaCuenta);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cuenta abierta exitosamente." });
            }

            // 3. Si sigue fallando, esto te dirá EXACTAMENTE qué campo da error en la consola
            var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Datos inválidos: " + string.Join(", ", errores) });
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


        [HttpPost]
        public async Task<IActionResult> RegistrarTransaccion(int cuentaId, decimal monto, string tipo, string descripcion)
        {
            // 1. Buscamos la cuenta incluyendo el rastreo para modificación
            var cuenta = await _context.Cuentas.FirstOrDefaultAsync(c => c.Id == cuentaId);

            if (cuenta == null) return Json(new { success = false, message = "Cuenta no encontrada." });

            // 2. Mapeo de lógica según tu Enum (Credito / Abono)
            // Usamos el valor que viene del HTML ("Credito" o "Abono")
            if (tipo == "Credito")
            {
                if (cuenta.SaldoActual + monto > cuenta.MontoMaximoCredito)
                {
                    return Json(new { success = false, message = "¡Error! Esta compra excede el límite de crédito disponible." });
                }
                cuenta.SaldoActual += monto;
            }
            else if (tipo == "Abono")
            {
                cuenta.SaldoActual -= monto;
                if (cuenta.SaldoActual < 0) cuenta.SaldoActual = 0;
            }

            // 3. Notificamos a EF que la cuenta ha cambiado (Importante)
            _context.Cuentas.Update(cuenta);

            // 4. Obtenemos el ID del usuario
            var idDelUsuarioLogueado = _userManager.GetUserId(User);

            // 5. Creamos la transacción
            var transac = new Transaccion
            {
                CuentaId = cuentaId,
                Monto = monto,
                Tipo = Enum.Parse<SistemaPulperia.Models.Enums.TipoTransaccion>(tipo),
                Descripcion = descripcion,
                Fecha = DateTime.Now,
                UsuarioId = idDelUsuarioLogueado
            };

            // 6. Guardamos todo en una sola operación atómica
            _context.Transacciones.Add(transac);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Se registró el {tipo} por C$ {monto} correctamente." });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerHistorial(int id)
        {
            var historial = await _context.Transacciones
                .Where(t => t.CuentaId == id)
                .Include(t => t.Usuario) // Traemos los datos del Admin/Usuario
                .OrderByDescending(t => t.Fecha) // Lo más reciente primero
                .Select(t => new
                {
                    fecha = t.Fecha.ToString("dd/MM/yyyy hh:mm tt"),
                    tipo = t.Tipo.ToString(),
                    descripcion = t.Descripcion ?? "Sin descripción",
                    monto = t.Monto,
                    registradoPor = t.Usuario.UserName // O t.Usuario.NombreCompleto si lo tienes
                })
                .ToListAsync();

            return Json(new { data = historial });
        }


    }
}