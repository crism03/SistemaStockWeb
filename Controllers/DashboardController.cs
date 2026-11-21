using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaStockWeb.Data;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.Enum;
using SistemaStockWeb.Models.ViewModels;
using SistemaStockWeb.Models.ViewModels.Movimiento;

namespace SistemaStockWeb.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private const int MaximoAlertas = 15;
    private const int CantidadUltimosMovimientos = 8;

    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public DashboardController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: Dashboard
    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);

        // El stock total no es una columna de la base: se calcula en memoria sobre los productos cargados.
        var productos = await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Stocks)
            .Where(p => p.Activo)
            .ToListAsync();

        var alertas = productos
            .Where(p => p.BajoStockMinimo)
            .OrderBy(p => p.StockTotal)
            .ThenBy(p => p.Nombre)
            .ToList();

        var ultimosMovimientos = await _context.Movimientos
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.DepositoOrigen)
            .Include(m => m.DepositoDestino)
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.id)
            .Take(CantidadUltimosMovimientos)
            .ToListAsync();

        var stockPorDeposito = await _context.StockDepositos
            .AsNoTracking()
            .GroupBy(s => s.Deposito!.Nombre)
            .Select(g => new StockChartItemViewModel { Etiqueta = g.Key, Valor = g.Sum(s => s.Cantidad) })
            .ToListAsync();

        var model = new DashboardViewModel
        {
            TotalProductosActivos = productos.Count,
            ValorTotalInventario = productos.Sum(p => p.StockTotal * p.PrecioCosto),
            TotalProveedoresActivos = await _context.Proveedores.CountAsync(p => p.Activo),
            TotalClientesActivos = await _context.Clientes.CountAsync(c => c.Activo),
            TotalDepositosActivos = await _context.Depositos.CountAsync(d => d.Activo),
            MovimientosHoy = await _context.Movimientos.CountAsync(m => m.Fecha >= hoy && m.Fecha < manana),
            ValorVentasHoy = await _context.Movimientos
                .Where(m => m.Tipo == TipoMovimiento.Salida && m.Fecha >= hoy && m.Fecha < manana)
                .SumAsync(m => m.Cantidad * m.Producto!.PrecioVenta),

            TotalAlertas = alertas.Count,
            AlertasReposicion = alertas.Take(MaximoAlertas).Select(p => new AlertaReposicionViewModel
            {
                idProducto = p.id,
                Producto = p.Nombre,
                Categoria = p.Categoria.Nombre,
                StockTotal = p.StockTotal,
                StockMinimo = p.StockMinimo
            }).ToList(),

            UltimosMovimientos = _mapper.Map<List<MovimientoViewModel>>(ultimosMovimientos),

            TopMayorStock = productos.OrderByDescending(p => p.StockTotal).Take(5).Select(ToItemGrafico).ToList(),
            TopMenorStock = productos.OrderBy(p => p.StockTotal).Take(5).Select(ToItemGrafico).ToList(),
            StockPorDeposito = stockPorDeposito.OrderByDescending(x => x.Valor).ToList()
        };

        return View(model);
    }

    private static StockChartItemViewModel ToItemGrafico(Producto p) =>
        new() { Etiqueta = p.Nombre, Valor = p.StockTotal };
}