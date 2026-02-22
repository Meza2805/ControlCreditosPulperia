using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPulperia.Models;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login() => View();

    // VALIDACIÓN AJAX: Verifica si el usuario existe antes de pedir la clave
    [HttpGet]
    public async Task<IActionResult> ValidarUsuario(string email)
    {
        var user = await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);
        if (user == null) return Json(new { existe = false });

        // Obtenemos su rol para el saludo posterior
        var roles = await _userManager.GetRolesAsync(user);
        return Json(new { existe = true, nombre = user.UserName, rol = roles.FirstOrDefault() ?? "Usuario" });
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        // 1. Buscamos al usuario
        var user = await _userManager.FindByEmailAsync(email) ?? await _userManager.FindByNameAsync(email);
        if (user == null) return Json(new { success = false, message = "Credenciales inválidas." });

        // 2. Intentamos el login (lockoutOnFailure: true activa el bloqueo automático)
        // El bloqueo se configura en Program.cs (lo haremos a 3 intentos)
        var result = await _signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return Json(new { success = true, user = user.UserName, role = roles.FirstOrDefault() });
        }

        if (result.IsLockedOut)
            return Json(new { success = false, message = "Cuenta bloqueada por seguridad. Contacte al Admin." });

        return Json(new { success = false, message = "Contraseña incorrecta." });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // 1. Limpia la cookie de autenticación
        await _signInManager.SignOutAsync();

        // 2. Redirige explícitamente a la raíz (Home/Index)
        // Esto hará que el Layout vuelva a evaluar User.Identity.IsAuthenticated como 'false'
        return RedirectToAction("Index", "Home");
    }
}