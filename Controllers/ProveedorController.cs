using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaStockWeb.Data;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.ViewModels;

namespace SistemaStockWeb.Controllers;

[Authorize(Roles = "Administrador,Encargado")]
public class ProveedorController : Controller
{
    private readonly AppDbContext _context;

    public ProveedorController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Proveedor
    public async Task<IActionResult> Index()
    {
        var proveedores = await _context.Proveedores.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
        return View(proveedores.Select(ToViewModel).ToList());
    }

    // GET: Proveedor/Create
    public IActionResult Create()
    {
        return View(new ProveedorViewModel());
    }

    // POST: Proveedor/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProveedorViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await ExisteDocumentoAsync(model.Documento))
        {
            ModelState.AddModelError(nameof(model.Documento), "Ya existe un proveedor con este documento.");
            return View(model);
        }

        var proveedor = new Proveedor();
        AplicarCambios(proveedor, model);
        _context.Proveedores.Add(proveedor);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Proveedor creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Proveedor/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var proveedor = await _context.Proveedores.AsNoTracking().FirstOrDefaultAsync(p => p.id == id);
        if (proveedor == null) return NotFound();

        return View(ToViewModel(proveedor));
    }

    // POST: Proveedor/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProveedorViewModel model)
    {
        if (id != model.id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null) return NotFound();

        if (await ExisteDocumentoAsync(model.Documento, id))
        {
            ModelState.AddModelError(nameof(model.Documento), "Ya existe un proveedor con este documento.");
            return View(model);
        }

        AplicarCambios(proveedor, model);
        proveedor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Proveedor actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Proveedor/Delete/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null) return NotFound();

        if (await _context.Movimientos.AnyAsync(m => m.idProveedor == id))
        {
            TempData["Error"] = $"No se puede eliminar '{proveedor.Nombre}' porque tiene movimientos registrados. Podés desactivarlo desde Editar.";
            return RedirectToAction(nameof(Index));
        }

        _context.Proveedores.Remove(proveedor);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Proveedor eliminado con éxito.";
        return RedirectToAction(nameof(Index));
    }

    private static ProveedorViewModel ToViewModel(Proveedor p) => new()
    {
        id = p.id,
        Nombre = p.Nombre,
        Contacto = p.Contacto,
        Documento = p.Documento,
        Telefono = p.Telefono,
        Email = p.Email,
        Direccion = p.Direccion,
        Observaciones = p.Observaciones,
        Activo = p.Activo
    };

    private static void AplicarCambios(Proveedor proveedor, ProveedorViewModel model)
    {
        proveedor.Nombre = model.Nombre.Trim();
        proveedor.Contacto = model.Contacto;
        proveedor.Documento = model.Documento?.Trim();
        proveedor.Telefono = model.Telefono;
        proveedor.Email = model.Email;
        proveedor.Direccion = model.Direccion;
        proveedor.Observaciones = model.Observaciones;
        proveedor.Activo = model.Activo;
    }

    /// <summary>Verifica si otro proveedor ya usa el mismo documento (solo si se informó uno).</summary>
    private async Task<bool> ExisteDocumentoAsync(string? documento, int idExcluido = 0)
    {
        if (string.IsNullOrWhiteSpace(documento)) return false;

        var documentoNormalizado = documento.Trim().ToLower();
        return await _context.Proveedores.AnyAsync(p =>
            p.id != idExcluido && p.Documento != null && p.Documento.ToLower() == documentoNormalizado);
    }
}
