using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaStockWeb.Data;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.ViewModels;

namespace SistemaStockWeb.Controllers;

[Authorize(Roles = "Administrador,Encargado")]
public class DepositoController : Controller
{
    private readonly AppDbContext _context;

    public DepositoController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Deposito
    public async Task<IActionResult> Index()
    {
        var depositos = await _context.Depositos.AsNoTracking().OrderBy(d => d.Nombre).ToListAsync();
        return View(depositos.Select(ToViewModel).ToList());
    }

    // GET: Deposito/Create
    public IActionResult Create()
    {
        return View(new DepositoViewModel());
    }

    // POST: Deposito/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepositoViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await ExisteNombreAsync(model.Nombre))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe un depósito con este nombre.");
            return View(model);
        }

        var deposito = new Deposito();
        AplicarCambios(deposito, model);
        _context.Depositos.Add(deposito);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Depósito creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Deposito/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var deposito = await _context.Depositos.AsNoTracking().FirstOrDefaultAsync(d => d.id == id);
        if (deposito == null) return NotFound();

        return View(ToViewModel(deposito));
    }

    // POST: Deposito/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DepositoViewModel model)
    {
        if (id != model.id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var deposito = await _context.Depositos.FindAsync(id);
        if (deposito == null) return NotFound();

        if (await ExisteNombreAsync(model.Nombre, id))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe un depósito con este nombre.");
            return View(model);
        }

        AplicarCambios(deposito, model);
        deposito.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Depósito actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Deposito/Delete/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deposito = await _context.Depositos.FindAsync(id);
        if (deposito == null) return NotFound();

        var tieneHistorial = await _context.StockDepositos.AnyAsync(s => s.idDeposito == id)
            || await _context.Movimientos.AnyAsync(m => m.idDepositoOrigen == id || m.idDepositoDestino == id);
        if (tieneHistorial)
        {
            TempData["Error"] = $"No se puede eliminar '{deposito.Nombre}' porque tiene stock o movimientos registrados. Podés desactivarlo desde Editar.";
            return RedirectToAction(nameof(Index));
        }

        _context.Depositos.Remove(deposito);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Depósito eliminado con éxito.";
        return RedirectToAction(nameof(Index));
    }

    private static DepositoViewModel ToViewModel(Deposito d) => new()
    {
        id = d.id,
        Nombre = d.Nombre,
        Ubicacion = d.Ubicacion,
        Activo = d.Activo
    };

    private static void AplicarCambios(Deposito deposito, DepositoViewModel model)
    {
        deposito.Nombre = model.Nombre.Trim();
        deposito.Ubicacion = model.Ubicacion?.Trim();
        deposito.Activo = model.Activo;
    }

    private Task<bool> ExisteNombreAsync(string nombre, int idExcluido = 0)
    {
        var nombreNormalizado = nombre.Trim().ToLower();
        return _context.Depositos.AnyAsync(d => d.id != idExcluido && d.Nombre.ToLower() == nombreNormalizado);
    }
}