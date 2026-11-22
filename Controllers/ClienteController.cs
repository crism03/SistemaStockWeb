using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaStockWeb.Data;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.ViewModels;

namespace SistemaStockWeb.Controllers;

[Authorize(Roles = "Administrador,Encargado")]
public class ClienteController : Controller
{
    private readonly AppDbContext _context;

    public ClienteController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Cliente
    public async Task<IActionResult> Index()
    {
        var clientes = await _context.Clientes.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync();
        return View(clientes.Select(ToViewModel).ToList());
    }

    // GET: Cliente/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.id == id);
        if (cliente == null) return NotFound();

        return View(ToViewModel(cliente));
    }

    // GET: Cliente/Create
    public IActionResult Create()
    {
        return View(new ClienteViewModel());
    }

    // POST: Cliente/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteViewModel model)
    {
        if (!ModelState.IsValid || await HayDuplicadoAsync(model)) return View(model);

        var cliente = new Cliente();
        AplicarCambios(cliente, model);
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Cliente creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Cliente/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.id == id);
        if (cliente == null) return NotFound();

        return View(ToViewModel(cliente));
    }

    // POST: Cliente/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteViewModel model)
    {
        if (id != model.id) return NotFound();
        if (!ModelState.IsValid || await HayDuplicadoAsync(model)) return View(model);

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        AplicarCambios(cliente, model);
        cliente.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Cliente/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.id == id);
        if (cliente == null) return NotFound();

        return View(ToViewModel(cliente));
    }

    // POST: Cliente/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        if (await _context.Movimientos.AnyAsync(m => m.idCliente == id))
        {
            TempData["Error"] = $"No se puede eliminar '{cliente.Nombre}' porque tiene movimientos registrados. Podés desactivarlo desde Editar.";
            return RedirectToAction(nameof(Index));
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Cliente eliminado con éxito.";
        return RedirectToAction(nameof(Index));
    }

    private static ClienteViewModel ToViewModel(Cliente c) => new()
    {
        id = c.id,
        Nombre = c.Nombre,
        Contacto = c.Contacto,
        Documento = c.Documento,
        Telefono = c.Telefono,
        Email = c.Email,
        Direccion = c.Direccion,
        Observaciones = c.Observaciones,
        Activo = c.Activo
    };

    private static void AplicarCambios(Cliente cliente, ClienteViewModel model)
    {
        cliente.Nombre = model.Nombre.Trim();
        cliente.Contacto = model.Contacto;
        cliente.Documento = model.Documento?.Trim();
        cliente.Telefono = model.Telefono;
        cliente.Email = model.Email;
        cliente.Direccion = model.Direccion;
        cliente.Observaciones = model.Observaciones;
        cliente.Activo = model.Activo;
    }

    /// <summary>Valida que no exista otro cliente con el mismo nombre o documento, sin distinguir mayúsculas.</summary>
    private async Task<bool> HayDuplicadoAsync(ClienteViewModel model)
    {
        var hayDuplicado = false;
        var nombre = model.Nombre.Trim().ToLower();

        if (await _context.Clientes.AnyAsync(c => c.id != model.id && c.Nombre.ToLower() == nombre))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe un cliente con este nombre.");
            hayDuplicado = true;
        }

        if (!string.IsNullOrWhiteSpace(model.Documento))
        {
            var documento = model.Documento.Trim().ToLower();
            if (await _context.Clientes.AnyAsync(c => c.id != model.id && c.Documento != null && c.Documento.ToLower() == documento))
            {
                ModelState.AddModelError(nameof(model.Documento), "Ya existe un cliente con este documento.");
                hayDuplicado = true;
            }
        }

        return hayDuplicado;
    }
}