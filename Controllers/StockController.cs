using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaStockWeb.Data;
using SistemaStockWeb.Models.ViewModels;

namespace SistemaStockWeb.Controllers;

[Authorize]
public class StockController : Controller
{
    private readonly AppDbContext _context;

    public StockController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Stock
    public async Task<IActionResult> Index()
    {
        var depositos = await _context.Depositos
            .AsNoTracking()
            .OrderBy(d => d.Nombre)
            .Select(d => new DepositoViewModel { id = d.id, Nombre = d.Nombre, Activo = d.Activo })
            .ToListAsync();

        var productos = await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Stocks)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        var model = new StockViewModel
        {
            Depositos = depositos,
            Productos = productos.Select(p => new StockProductoViewModel
            {
                id = p.id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                CategoriaNombre = p.Categoria.Nombre,
                UnidadDeMedida = p.UnidadDeMedida,
                StockMinimo = p.StockMinimo,
                Total = p.StockTotal,
                BajoMinimo = p.BajoStockMinimo,
                CantidadPorDeposito = p.Stocks.ToDictionary(s => s.idDeposito, s => s.Cantidad)
            }).ToList()
        };

        return View(model);
    }
}
