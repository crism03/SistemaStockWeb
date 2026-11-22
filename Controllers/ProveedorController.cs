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

    // GET: Proveedor/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var proveedor = await _context.Proveedores.AsNoTracking().FirstOrDefaultAsync(p => p.id == id);
        if (proveedor == null) return NotFound();

        return View(ToViewModel(proveedor));
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
        if (!ModelState.IsValid || await HayDuplicadoAsync(model)) return View(model);

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
        if (!ModelState.IsValid || await HayDuplicadoAsync(model)) return View(model);

        var proveedor = await _context.Proveedores.FindAsync(id);
        if (proveedor == null) return NotFound();

        AplicarCambios(proveedor, model);
        proveedor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Proveedor actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Proveedor/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var proveedor = await _context.Proveedores.AsNoTracking().FirstOrDefaultAsync(p => p.id == id);
        if (proveedor == null) return NotFound();

        return View(ToViewModel(proveedor));
    }

    // POST: Proveedor/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
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

    /// <summary>Valida que no exista otro proveedor con el mismo nombre o documento, sin distinguir mayúsculas.</summary>
    private async Task<bool> HayDuplicadoAsync(ProveedorViewModel model)
    {
        var hayDuplicado = false;
        var nombre = model.Nombre.Trim().ToLower();

        if (await _context.Proveedores.AnyAsync(p => p.id != model.id && p.Nombre.ToLower() == nombre))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe un proveedor con este nombre.");
            hayDuplicado = true;
        }

        if (!string.IsNullOrWhiteSpace(model.Documento))
        {
            var documento = model.Documento.Trim().ToLower();
            if (await _context.Proveedores.AnyAsync(p => p.id != model.id && p.Documento != null && p.Documento.ToLower() == documento))
            {
                ModelState.AddModelError(nameof(model.Documento), "Ya existe un proveedor con este documento.");
                hayDuplicado = true;
            }
        }

        return hayDuplicado;
    }
}