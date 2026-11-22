using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaStockWeb.Data;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.ViewModels;

namespace SistemaStockWeb.Controllers;

[Authorize(Roles = "Administrador,Encargado")]
public class CategoriaController : Controller
{
    private readonly AppDbContext _context;

    public CategoriaController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Categoria
    public async Task<IActionResult> Index()
    {
        var categorias = await ConsultarViewModels().OrderBy(c => c.Nombre).ToListAsync();
        return View(categorias);
    }

    // GET: Categoria/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var model = await ConsultarViewModels().FirstOrDefaultAsync(c => c.id == id);
        if (model == null) return NotFound();

        return View(model);
    }

    // GET: Categoria/Create
    public IActionResult Create()
    {
        return View(new CategoriaViewModel());
    }

    // POST: Categoria/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoriaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (await ExisteNombreAsync(model.Nombre))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe una categoría con este nombre.");
            return View(model);
        }

        _context.Categorias.Add(new Categoria
        {
            Nombre = model.Nombre.Trim(),
            Descripcion = model.Descripcion,
            Activo = model.Activo
        });
        await _context.SaveChangesAsync();

        TempData["Ok"] = "La categoría se creó con éxito.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Categoria/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var model = await ConsultarViewModels().FirstOrDefaultAsync(c => c.id == id);
        if (model == null) return NotFound();

        return View(model);
    }

    // POST: Categoria/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoriaViewModel model)
    {
        if (id != model.id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();

        if (await ExisteNombreAsync(model.Nombre, id))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe una categoría con este nombre.");
            return View(model);
        }

        categoria.Nombre = model.Nombre.Trim();
        categoria.Descripcion = model.Descripcion;
        categoria.Activo = model.Activo;
        categoria.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Ok"] = "La categoría se modificó con éxito.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Categoria/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var model = await ConsultarViewModels().FirstOrDefaultAsync(c => c.id == id);
        if (model == null) return NotFound();

        return View(model);
    }

    // POST: Categoria/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();

        if (await _context.Productos.AnyAsync(p => p.idCategoria == id))
        {
            TempData["Error"] = $"No se puede eliminar la categoría '{categoria.Nombre}' porque tiene productos asociados.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Categoría eliminada con éxito.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Consulta base que arma los ViewModels (con la cantidad de productos) directamente en la base.</summary>
    private IQueryable<CategoriaViewModel> ConsultarViewModels() =>
        _context.Categorias.AsNoTracking().Select(c => new CategoriaViewModel
        {
            id = c.id,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion,
            Activo = c.Activo,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            CantidadProductos = c.Productos.Count
        });

    /// <summary>Verifica si existe otra categoría con el mismo nombre, sin distinguir mayúsculas.</summary>
    private Task<bool> ExisteNombreAsync(string nombre, int idExcluido = 0)
    {
        var nombreNormalizado = nombre.Trim().ToLower();
        return _context.Categorias.AnyAsync(c => c.id != idExcluido && c.Nombre.ToLower() == nombreNormalizado);
    }
}