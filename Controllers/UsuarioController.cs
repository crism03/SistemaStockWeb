using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.ViewModels.Usuario;

namespace SistemaStockWeb.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuarioController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsuarioController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ---------- LISTADO ----------

    public async Task<IActionResult> Index()
    {
        var usuarios = await _userManager.Users.OrderBy(u => u.Nombre).ToListAsync();
        var lista = new List<UsuarioViewModel>();

        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            lista.Add(new UsuarioViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email ?? string.Empty,
                Rol = roles.FirstOrDefault() ?? "Sin rol",
                Activo = usuario.Activo
            });
        }

        return View(lista);
    }

    // ---------- CREAR ----------

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        ViewBag.Roles = await CargarRoles();
        return View(new CrearUsuarioViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearUsuarioViewModel model)
    {
        if (ModelState.IsValid && await CrearUsuarioAsync(model))
        {
            TempData["Ok"] = "Usuario creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Roles = await CargarRoles(model.Rol);
        return View(model);
    }

    // ---------- EDITAR ----------

    [HttpGet]
    public async Task<IActionResult> Editar(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(usuario);
        ViewBag.Roles = await CargarRoles(roles.FirstOrDefault());

        return View(new EditarUsuarioViewModel
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email ?? string.Empty,
            Rol = roles.FirstOrDefault() ?? string.Empty,
            Activo = usuario.Activo
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarUsuarioViewModel model)
    {
        var usuario = await _userManager.FindByIdAsync(model.Id);
        if (usuario == null) return NotFound();

        if (ModelState.IsValid && await ActualizarUsuarioAsync(usuario, model))
        {
            TempData["Ok"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Roles = await CargarRoles(model.Rol);
        return View(model);
    }

    // ---------- RESETEAR PASSWORD ----------

    [HttpGet]
    public async Task<IActionResult> ResetearPassword(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        return View(new ResetearPasswordViewModel { Id = usuario.Id, Email = usuario.Email ?? string.Empty });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetearPassword(ResetearPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var usuario = await _userManager.FindByIdAsync(model.Id);
        if (usuario == null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
        var resultado = await _userManager.ResetPasswordAsync(usuario, token, model.NuevaPassword);
        if (!resultado.Succeeded)
        {
            AgregarErrores(resultado);
            return View(model);
        }

        TempData["Ok"] = "Contraseña restablecida correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- MÉTODOS PRIVADOS ----------

    private async Task<bool> CrearUsuarioAsync(CrearUsuarioViewModel model)
    {
        if (!await _roleManager.RoleExistsAsync(model.Rol))
        {
            ModelState.AddModelError(nameof(model.Rol), "El rol seleccionado no existe.");
            return false;
        }

        var usuario = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            Nombre = model.Nombre,
            EmailConfirmed = true,
            Activo = true,
            UsuarioRegistro = User.Identity?.Name
        };

        var resultado = await _userManager.CreateAsync(usuario, model.Password);
        if (!resultado.Succeeded)
        {
            AgregarErrores(resultado);
            return false;
        }

        // Si no se pudo asignar el rol, se elimina el usuario para no dejar cuentas sin rol.
        var resultadoRol = await _userManager.AddToRoleAsync(usuario, model.Rol);
        if (!resultadoRol.Succeeded)
        {
            await _userManager.DeleteAsync(usuario);
            AgregarErrores(resultadoRol);
            return false;
        }

        return true;
    }

    private async Task<bool> ActualizarUsuarioAsync(ApplicationUser usuario, EditarUsuarioViewModel model)
    {
        if (!await _roleManager.RoleExistsAsync(model.Rol))
        {
            ModelState.AddModelError(nameof(model.Rol), "El rol seleccionado no existe.");
            return false;
        }

        var esUsuarioActual = usuario.Id == _userManager.GetUserId(User);
        if (esUsuarioActual && !model.Activo)
        {
            ModelState.AddModelError(string.Empty, "No puedes desactivar tu propio usuario.");
            return false;
        }
        if (esUsuarioActual && model.Rol != "Administrador")
        {
            ModelState.AddModelError(string.Empty, "No puedes quitarte el rol de Administrador.");
            return false;
        }

        usuario.Nombre = model.Nombre;
        usuario.Email = model.Email;
        usuario.UserName = model.Email;
        usuario.Activo = model.Activo;
        usuario.UpdatedAt = DateTime.Now;

        var resultado = await _userManager.UpdateAsync(usuario);
        if (!resultado.Succeeded)
        {
            AgregarErrores(resultado);
            return false;
        }

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        if (rolesActuales.Count == 1 && rolesActuales[0] == model.Rol) return true;

        await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
        resultado = await _userManager.AddToRoleAsync(usuario, model.Rol);
        if (!resultado.Succeeded)
        {
            AgregarErrores(resultado);
            return false;
        }

        return true;
    }

    private async Task<SelectList> CargarRoles(string? rolSeleccionado = null)
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        return new SelectList(roles, "Name", "Name", rolSeleccionado);
    }

    private void AgregarErrores(IdentityResult resultado)
    {
        foreach (var error in resultado.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }
}