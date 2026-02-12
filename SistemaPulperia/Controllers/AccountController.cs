using System.Diagnostics;
using System.Security.Claims; // <--- Necesario para ClaimTypes.Email
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaPulperia.Models;

namespace SistemaPulperia.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    // 1. Declaramos los Managers para Identity
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    // 2. Los inyectamos en el constructor
    public AccountController(
        ILogger<AccountController> logger,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // 3. AGREGAMOS LA ACCIÓN LOGIN (Esto quita el error de la línea roja)
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        // Si el usuario ya está autenticado, lo mandamos al Home
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        // 1. Obtener la información del login externo (Google)
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["AlertMessage"] = "Error al obtener información de la cuenta externa.";
            return RedirectToAction(nameof(Login));
        }

        // 2. Obtener el email de los Claims de Google
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            TempData["AlertMessage"] = "No se pudo obtener el correo desde Google.";
            return RedirectToAction(nameof(Login));
        }

        // 3. Lógica de "Lista Blanca": Verificar si el usuario existe en nuestra BD
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // BLOQUEO: Si no está registrado tú previamente, no entra.
            TempData["AlertTitle"] = "Acceso Denegado";
            TempData["AlertMessage"] = $"El correo {email} no está autorizado. Contacte al administrador.";
            TempData["AlertIcon"] = "error";
            return RedirectToAction(nameof(Login));
        }

        // 4. Si el usuario existe, intentar el inicio de sesión
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        if (result.Succeeded)
        {
            // Actualizar auditoría
            user.UltimoAcceso = DateTime.Now;
            user.UltimaIP = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _userManager.UpdateAsync(user);

            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut) return RedirectToAction("Lockout");

        TempData["AlertMessage"] = "Error al intentar iniciar sesión con Google.";
        return RedirectToAction(nameof(Login));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}