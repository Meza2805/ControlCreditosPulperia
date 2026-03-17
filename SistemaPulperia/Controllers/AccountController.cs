using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Data;
using SistemaPulperia.Models;
using SistemaPulperia.Models.Entities; // Ajusta si tu ApplicationUser está aquí
using SistemaPulperia.Web.Models.Entities; // Ajusta si tu NivelAcceso está aquí
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaPulperia.Controllers
{
    // [Authorize(Roles = "Administrador")] // Descomenta esto cuando tengas tu sistema de login activo
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<NivelAcceso> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<NivelAcceso> roleManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        #region VISTAS DE AUTENTICACIÓN (LOGIN/LOGOUT)

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public async Task<IActionResult> ValidarUsuario(string email)
        {
            var user = await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);
            if (user == null) return Json(new { existe = false });

            var roles = await _userManager.GetRolesAsync(user);
            return Json(new { existe = true, nombre = user.UserName, rol = roles.FirstOrDefault() ?? "Usuario" });
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);
            if (user == null) return Json(new { success = false, message = "Credenciales inválidas." });

            var result = await _signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                return Json(new { success = true, user = user.UserName, role = roles.FirstOrDefault() });
            }

            if (result.IsLockedOut)
                return Json(new { success = false, message = "Cuenta bloqueada temporalmente por seguridad." });

            return Json(new { success = false, message = "Contraseña incorrecta." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region GESTIÓN DE USUARIOS (CRUD)

        // 1. VISTA PRINCIPAL (Carga los roles para el Modal de Edición)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Traemos los roles para llenar el <select> del modal de edición
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();

            return View();
        }

        // 2. DATA PARA EL DATATABLE (AJAX)
        [HttpGet]
        public async Task<IActionResult> ListarUsuarios()
        {
            var usuarios = await _userManager.Users.ToListAsync();
            var listaData = new List<object>();

            foreach (var user in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(user);
                listaData.Add(new
                {
                    id = user.Id,
                    nombreCompleto = user.NombreCompleto,
                    userName = user.UserName,
                    email = user.Email,
                    rol = roles.FirstOrDefault() ?? "Sin Rol",
                    // Verificamos si la fecha de bloqueo es a futuro (Usando UTC para evitar bugs horarios)
                    bloqueado = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow
                });
            }
            return Json(new { data = listaData });
        }

        // 3. VISTA REGISTRAR NUEVO USUARIO (GET)
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();

            return View();
        }

        // 4. PROCESAR REGISTRO (POST)
        [HttpPost]
        public async Task<IActionResult> Registrar(string nombre, string username, string email, string password, string rol)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return Json(new { success = false, message = "La contraseña debe tener al menos 6 caracteres." });

            var nuevoUsuario = new ApplicationUser
            {
                NombreCompleto = nombre,
                UserName = username,
                Email = email,
                EmailConfirmed = true,
                FechaRegistro = DateTime.Now
            };

            var resultado = await _userManager.CreateAsync(nuevoUsuario, password);

            if (resultado.Succeeded)
            {
                if (!string.IsNullOrEmpty(rol))
                {
                    await _userManager.AddToRoleAsync(nuevoUsuario, rol);
                }
                return Json(new { success = true, message = "Usuario registrado exitosamente." });
            }

            var errorMsg = resultado.Errors.FirstOrDefault()?.Description ?? "Error al crear el usuario.";
            return Json(new { success = false, message = errorMsg });
        }

        #endregion

        #region ACCIONES RÁPIDAS (AJAX Y MODALS)

        // 1. Obtener datos de un usuario para llenar el Modal de Edición (Sustituye al GET Editar antiguo)
        [HttpGet]
        public async Task<IActionResult> ObtenerUsuario(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(new { success = false, message = "ID inválido." });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return Json(new { success = false, message = "Usuario no encontrado." });

            var roles = await _userManager.GetRolesAsync(user);

            return Json(new
            {
                success = true,
                data = new
                {
                    id = user.Id,
                    nombreCompleto = user.NombreCompleto,
                    email = user.Email,
                    rol = roles.FirstOrDefault() ?? ""
                }
            });
        }

        // 2. Guardar los cambios desde el Modal de Edición
        [HttpPost]
        public async Task<IActionResult> Editar(string id, string nombre, string email, string rol)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return Json(new { success = false, message = "Usuario no encontrado." });

            // Actualizamos los datos básicos
            user.NombreCompleto = nombre;
            user.Email = email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Actualizamos el rol si es diferente al que ya tenía
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (!currentRoles.Contains(rol) && !string.IsNullOrEmpty(rol))
                {
                    // Removemos roles anteriores y agregamos el nuevo
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }
                    await _userManager.AddToRoleAsync(user, rol);
                }

                return Json(new { success = true, message = "Usuario actualizado exitosamente." });
            }

            var errorMsg = result.Errors.FirstOrDefault()?.Description ?? "Error al actualizar el usuario.";
            return Json(new { success = false, message = errorMsg });
        }

        // 3. Resetear Contraseña
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            // 1. Verificación de existencia
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

            // 2. OBTENER EL HISTORIAL (Últimas 5 contraseñas)
            var historial = await _context.HistorialContrasenas
                .Where(h => h.UsuarioId == id)
                .OrderByDescending(h => h.FechaRegistro)
                .Take(5)
                .ToListAsync();

            var hasher = _userManager.PasswordHasher;

            // 3. COMPARAR CONTRA EL HISTORIAL (Seguridad Avanzada)
            foreach (var registro in historial)
            {
                var resultadoComparacion = hasher.VerifyHashedPassword(usuario, registro.PasswordHash, newPassword);

                if (resultadoComparacion == PasswordVerificationResult.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = "¡Política de Seguridad! No puedes usar una contraseña que hayas utilizado recientemente."
                    });
                }
            }

            // 4. EJECUTAR EL CAMBIO
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultadoReset = await _userManager.ResetPasswordAsync(usuario, token, newPassword);

            if (resultadoReset.Succeeded)
            {
                // 5. REGISTRAR EN EL HISTORIAL EL NUEVO HASH
                var nuevoRegistro = new HistorialContrasena
                {
                    UsuarioId = id,
                    PasswordHash = hasher.HashPassword(usuario, newPassword),
                    FechaRegistro = DateTime.Now
                };

                _context.HistorialContrasenas.Add(nuevoRegistro);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Contraseña actualizada exitosamente." });
            }

            // Si Identity rechaza la clave por falta de mayúsculas/números (Program.cs)
            var errorMsg = resultadoReset.Errors.FirstOrDefault()?.Description ?? "Error al actualizar.";
            return Json(new { success = false, message = errorMsg });
        }


        // 4. Bloquear / Desbloquear Usuario
        [HttpPost]
        public async Task<IActionResult> ToggleBloqueo(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

            if (usuario.LockoutEnd.HasValue && usuario.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                // Desbloquear: Removemos la fecha de bloqueo
                await _userManager.SetLockoutEndDateAsync(usuario, null);
                return Json(new { success = true, message = "Usuario desbloqueado correctamente." });
            }
            else
            {
                // Bloquear: Ponemos una fecha muy lejana (ej: 100 años)
                await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.UtcNow.AddYears(100));
                return Json(new { success = true, message = "Usuario bloqueado. Ya no podrá iniciar sesión." });
            }
        }

        // 5. Eliminar Usuario
        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

            // Protección vital: Evitar que el admin activo se elimine a sí mismo por accidente
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && usuario.Id == currentUser.Id)
            {
                return Json(new { success = false, message = "Por seguridad, no puedes eliminar tu propio usuario." });
            }

            var resultado = await _userManager.DeleteAsync(usuario);
            if (resultado.Succeeded)
                return Json(new { success = true, message = "Usuario eliminado del sistema." });

            return Json(new { success = false, message = "No se pudo eliminar el usuario." });
        }
        [HttpGet]
        public async Task<IActionResult> ValidarDisponibilidad(string campo, string valor)
        {
            bool existe = false;
            if (campo == "username")
            {
                existe = await _userManager.FindByNameAsync(valor) != null;
            }
            else if (campo == "email")
            {
                existe = await _userManager.FindByEmailAsync(valor) != null;
            }

            return Json(new { disponible = !existe });
        }


        #endregion
    }
}