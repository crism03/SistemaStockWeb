using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.ViewModels.Cuenta;

namespace SistemaStockWeb.Controllers;

public class CuentaController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public CuentaController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // ---------- LOGIN / LOGOUT ----------

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        // Un usuario desactivado no puede ingresar, con el mismo mensaje genérico que una clave incorrecta.
        var usuario = await _userManager.FindByEmailAsync(model.Email);
        if (usuario == null || !usuario.Activo)
        {
            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
            return View(model);
        }

        var resultado = await _signInManager.PasswordSignInAsync(
            usuario, model.Password, model.RecordarSesion, lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            return Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, resultado.IsLockedOut
            ? "Cuenta bloqueada. Intente nuevamente en 10 minutos."
            : "Email o contraseña incorrectos.");
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccesoDenegado() => View();

    // ---------- CAMBIAR PASSWORD ----------

    [HttpGet, Authorize]
    public IActionResult CambiarPassword() => View(new CambiarPasswordViewModel());

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return RedirectToAction(nameof(Login));

        var resultado = await _userManager.ChangePasswordAsync(usuario, model.PasswordActual, model.NuevaPassword);
        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        // Mantiene la sesión abierta con la nueva contraseña.
        await _signInManager.RefreshSignInAsync(usuario);
        TempData["Ok"] = "Contraseña actualizada.";
        return RedirectToAction("Index", "Home");
    }
}