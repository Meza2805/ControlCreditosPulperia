using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Models;
using SistemaPulperia.Models.Entities; // Asegúrate de que apunte a tu ApplicationUser
using SistemaPulperia.Web.Models.Entities; // Donde reside NivelAcceso

namespace SistemaPulperia.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<NivelAcceso> _roleManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<NivelAcceso> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
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

        // 1. LISTADO DE USUARIOS
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Traemos los usuarios para la tabla administrativa
            var usuarios = await _userManager.Users.ToListAsync();
            return View(usuarios);
        }

        // 2. VISTA REGISTRAR (GET)
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            // Cargamos los niveles de acceso (roles) para el select
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();

            return View();
        }

        // 3. PROCESAR REGISTRO (POST)
        [HttpPost]
        public async Task<IActionResult> Registrar(string nombre, string username, string email, string password, string rol)
        {
            // Validaciones básicas
            if (string.IsNullOrEmpty(password) || password.Length < 6)
                return Json(new { success = false, message = "La contraseña debe tener al menos 6 caracteres." });

            var nuevoUsuario = new ApplicationUser
            {
                NombreCompleto = nombre,
                UserName = username,
                Email = email,
                EmailConfirmed = true, // Confirmado por defecto para agilizar en la pulpería
                FechaRegistro = DateTime.Now
            };

            // Creamos el usuario con hashing automático de contraseña
            var resultado = await _userManager.CreateAsync(nuevoUsuario, password);

            if (resultado.Succeeded)
            {
                // Asignamos el rol (Nivel de Acceso)
                if (!string.IsNullOrEmpty(rol))
                {
                    await _userManager.AddToRoleAsync(nuevoUsuario, rol);
                }

                return Json(new { success = true, message = "Usuario registrado exitosamente." });
            }

            // Si falla, retornamos el primer error descriptivo de Identity
            var errorMsg = resultado.Errors.FirstOrDefault()?.Description ?? "Error al crear el usuario.";
            return Json(new { success = false, message = errorMsg });
        }
        #region ACCIONES DE GESTIÓN DE USUARIOS

        // 1. Resetear Contraseña (Lógica básica para portafolio)
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

            // En un sistema real se envía un token por correo. 
            // Aquí, por ser una pulpería, podemos resetearla a una clave genérica:
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultado = await _userManager.ResetPasswordAsync(usuario, token, "Pulperia2026*");

            if (resultado.Succeeded)
                return Json(new { success = true, message = "Clave restablecida a: Pulperia2026*" });

            return Json(new { success = false, message = "Error al restablecer clave." });
        }

        // 2. Bloquear / Desbloquear Usuario
        [HttpPost]
        public async Task<IActionResult> ToggleBloqueo(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

            if (usuario.LockoutEnd != null && usuario.LockoutEnd > DateTime.Now)
            {
                // Desbloquear: Ponemos la fecha de fin de bloqueo en el pasado
                await _userManager.SetLockoutEndDateAsync(usuario, DateTime.Now.AddMinutes(-1));
                return Json(new { success = true, message = "Usuario desbloqueado correctamente." });
            }
            else
            {
                // Bloquear: Ponemos una fecha muy lejana (ej. 100 años)
                await _userManager.SetLockoutEndDateAsync(usuario, DateTime.Now.AddYears(100));
                return Json(new { success = true, message = "Usuario bloqueado. Ya no podrá iniciar sesión." });
            }
        }

        // 3. Eliminar Usuario
        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

            var resultado = await _userManager.DeleteAsync(usuario);
            if (resultado.Succeeded)
                return Json(new { success = true, message = "Usuario eliminado del sistema." });

            return Json(new { success = false, message = "No se pudo eliminar el usuario." });
        }

        #endregion
        #endregion
    }
}


