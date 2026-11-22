using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaStockWeb.Data;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.ViewModels;

namespace SistemaStockWeb.Controllers;

[Authorize(Roles = "Administrador,Encargado")]
public class ProductoController : Controller
{
    private readonly AppDbContext _context;

    public ProductoController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Producto
    public async Task<IActionResult> Index(string? buscar, int? buscarCategoria)
    {
        var consulta = _context.Productos.AsNoTracking().Include(p => p.Categoria).AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
            consulta = consulta.Where(p => p.Nombre.Contains(buscar) || (p.Codigo != null && p.Codigo.Contains(buscar)));

        if (buscarCategoria.HasValue)
            consulta = consulta.Where(p => p.idCategoria == buscarCategoria);

        var productos = await consulta.OrderBy(p => p.Nombre).ToListAsync();

        ViewBag.Categorias = await ObtenerCategoriasAsync(buscarCategoria, soloActivas: false);
        ViewBag.Buscar = buscar;
        return View(productos.Select(ToViewModel).ToList());
    }

    // GET: Producto/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var producto = await _context.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.id == id);
        if (producto == null) return NotFound();

        return View(ToViewModel(producto));
    }

    // GET: Producto/Create
    public async Task<IActionResult> Create()
    {
        var model = new ProductoViewModel { Categorias = await ObtenerCategoriasAsync() };
        return View(model);
    }

    // POST: Producto/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductoViewModel model)
    {
        if (ModelState.IsValid && !await HayDuplicadoAsync(model))
        {
            var producto = new Producto { Nombre = model.Nombre.Trim() };
            AplicarCambios(producto, model);

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        model.Categorias = await ObtenerCategoriasAsync(model.idCategoria);
        return View(model);
    }

    // GET: Producto/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var producto = await _context.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.id == id);
        if (producto == null) return NotFound();

        var model = ToViewModel(producto);
        model.Categorias = await ObtenerCategoriasAsync(model.idCategoria);
        return View(model);
    }

    // POST: Producto/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductoViewModel model)
    {
        if (id != model.id) return NotFound();

        if (ModelState.IsValid && !await HayDuplicadoAsync(model))
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            AplicarCambios(producto, model);
            producto.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        model.Categorias = await ObtenerCategoriasAsync(model.idCategoria);
        return View(model);
    }

    // GET: Producto/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var producto = await _context.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.id == id);
        if (producto == null) return NotFound();

        return View(ToViewModel(producto));
    }

    // POST: Producto/Delete/5
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        var tieneHistorial = await _context.Movimientos.AnyAsync(m => m.idProducto == id)
            || await _context.StockDepositos.AnyAsync(s => s.idProducto == id);
        if (tieneHistorial)
        {
            TempData["Error"] = $"No se puede eliminar '{producto.Nombre}' porque tiene movimientos o stock registrado. Podés desactivarlo desde Editar.";
            return RedirectToAction(nameof(Index));
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        TempData["Ok"] = "Producto eliminado con éxito.";
        return RedirectToAction(nameof(Index));
    }

    private static ProductoViewModel ToViewModel(Producto p) => new()
    {
        id = p.id,
        Codigo = p.Codigo,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        idCategoria = p.idCategoria,
        CategoriaNombre = p.Categoria?.Nombre,
        UnidadDeMedida = p.UnidadDeMedida,
        PrecioCosto = p.PrecioCosto,
        PrecioVenta = p.PrecioVenta,
        StockMinimo = p.StockMinimo,
        Activo = p.Activo,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    
    private static void AplicarCambios(Producto producto, ProductoViewModel model)
    {
        producto.Codigo = model.Codigo?.Trim();
        producto.Nombre = model.Nombre.Trim();
        producto.Descripcion = model.Descripcion;
        producto.idCategoria = model.idCategoria!.Value;
        producto.UnidadDeMedida = model.UnidadDeMedida;
        producto.PrecioCosto = model.PrecioCosto;
        producto.PrecioVenta = model.PrecioVenta;
        producto.StockMinimo = model.StockMinimo;
        producto.Activo = model.Activo;
    }

    /// <summary>Valida que no exista otro producto con el mismo nombre o código. </summary>
    private async Task<bool> HayDuplicadoAsync(ProductoViewModel model)
    {
        var hayDuplicado = false;
        var nombre = model.Nombre.Trim().ToLower();
        var codigo = model.Codigo?.Trim().ToLower();

        if (await _context.Productos.AnyAsync(p => p.id != model.id && p.Nombre.ToLower() == nombre))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe un producto con este nombre.");
            hayDuplicado = true;
        }

        if (!string.IsNullOrEmpty(codigo)
            && await _context.Productos.AnyAsync(p => p.id != model.id && p.Codigo != null && p.Codigo.ToLower() == codigo))
        {
            ModelState.AddModelError(nameof(model.Codigo), "Ya existe un producto con este código.");
            hayDuplicado = true;
        }

        return hayDuplicado;
    }

    /// <summary>
    /// Opciones del combo de categorías. En los formularios se muestran las activas más la seleccionada;
    /// en el filtro del listado se muestran todas.
    /// </summary>
    private async Task<List<SelectListItem>> ObtenerCategoriasAsync(int? idSeleccionado = null, bool soloActivas = true)
    {
        return await _context.Categorias
            .AsNoTracking()
            .Where(c => !soloActivas || c.Activo || c.id == idSeleccionado)
            .OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem
            {
                Value = c.id.ToString(),
                Text = c.Nombre,
                Selected = c.id == idSeleccionado
            })
            .ToListAsync();
    }
}