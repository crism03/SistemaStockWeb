using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaStockWeb.Data;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.Enum;
using SistemaStockWeb.Models.ViewModels.Movimiento;

namespace SistemaStockWeb.Controllers;

[Authorize]
public class MovimientoController : Controller
{
    // El historial lo puede ver cualquier usuario; registrar movimientos requiere uno de estos roles.
    private const string RolesOperacion = "Administrador,Encargado,Operador";
    private const int MaximoHistorial = 200;

    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public MovimientoController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: Movimiento
    public async Task<IActionResult> Index(int? idProducto, TipoMovimiento? tipo)
    {
        var consulta = _context.Movimientos
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.DepositoOrigen)
            .Include(m => m.DepositoDestino)
            .Include(m => m.Proveedor)
            .Include(m => m.Cliente)
            .AsQueryable();

        if (idProducto.HasValue) consulta = consulta.Where(m => m.idProducto == idProducto);
        if (tipo.HasValue) consulta = consulta.Where(m => m.Tipo == tipo);

        var movimientos = await consulta
            .OrderByDescending(m => m.Fecha)
            .ThenByDescending(m => m.id)
            .Take(MaximoHistorial)
            .ToListAsync();

        ViewBag.Productos = await ObtenerProductosAsync(idProducto, soloActivos: false);
        ViewBag.MaximoHistorial = MaximoHistorial;
        return View(_mapper.Map<List<MovimientoViewModel>>(movimientos));
    }

    // ---------- ENTRADA ----------

    [Authorize(Roles = RolesOperacion)]
    public async Task<IActionResult> Entrada()
    {
        var model = new OperacionStockEntradaViewModel();
        await CargarListasAsync(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesOperacion)]
    public async Task<IActionResult> Entrada(OperacionStockEntradaViewModel model)
    {
        if (ModelState.IsValid)
        {
            var movimiento = CrearMovimiento(TipoMovimiento.Entrada, model);
            movimiento.idProveedor = model.idProveedor;

            var error = await RegistrarAsync(
                model.idProducto!.Value,
                new[] { (model.idDepositoOrigen!.Value, model.Cantidad) },
                movimiento);

            if (error == null)
            {
                TempData["Ok"] = "Entrada de stock registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, error);
        }

        await CargarListasAsync(model);
        return View(model);
    }

    // ---------- SALIDA ----------

    [Authorize(Roles = RolesOperacion)]
    public async Task<IActionResult> Salida()
    {
        var model = new OperacionStockSalidaViewModel();
        await CargarListasAsync(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesOperacion)]
    public async Task<IActionResult> Salida(OperacionStockSalidaViewModel model)
    {
        if (ModelState.IsValid)
        {
            var movimiento = CrearMovimiento(TipoMovimiento.Salida, model);
            movimiento.idCliente = model.idCliente;

            var error = await RegistrarAsync(
                model.idProducto!.Value,
                new[] { (model.idDepositoOrigen!.Value, -model.Cantidad) },
                movimiento);

            if (error == null)
            {
                TempData["Ok"] = "Salida de stock registrada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(string.Empty, error);
        }

        await CargarListasAsync(model);
        return View(model);
    }

    // ---------- TRANSFERENCIA ----------

    [Authorize(Roles = RolesOperacion)]
    public async Task<IActionResult> Transferencia()
    {
        var model = new TransferenciaViewModel();
        await CargarListasAsync(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = RolesOperacion)]
    public async Task<IActionResult> Transferencia(TransferenciaViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (model.idDepositoOrigen == model.idDepositoDestino)
            {
                ModelState.AddModelError(nameof(model.idDepositoDestino), "El depósito de destino debe ser distinto al de origen.");
            }
            else
            {
                // Una transferencia deja dos registros: la salida del origen y la entrada al destino.
                var salida = CrearMovimiento(TipoMovimiento.TransferenciaSalida, model);
                salida.idDepositoDestino = model.idDepositoDestino;

                var entrada = CrearMovimiento(TipoMovimiento.TransferenciaEntrada, model);
                entrada.idDepositoDestino = model.idDepositoDestino;

                var error = await RegistrarAsync(
                    model.idProducto!.Value,
                    new[]
                    {
                        (model.idDepositoOrigen!.Value, -model.Cantidad),
                        (model.idDepositoDestino!.Value, model.Cantidad)
                    },
                    salida, entrada);

                if (error == null)
                {
                    TempData["Ok"] = $"Transferencia de {model.Cantidad} unidades registrada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, error);
            }
        }

        await CargarListasAsync(model);
        return View(model);
    }

    // ---------- MÉTODOS PRIVADOS ----------

    private Movimiento CrearMovimiento(TipoMovimiento tipo, OperacionStockViewModel model) => new()
    {
        Tipo = tipo,
        idProducto = model.idProducto!.Value,
        idDepositoOrigen = model.idDepositoOrigen,
        Cantidad = model.Cantidad,
        Observaciones = model.Observaciones,
        Fecha = DateTime.Now,
        UsuarioRegistro = User.Identity?.Name ?? "sistema"
    };

    /// <summary>
    /// Aplica los ajustes de stock y guarda los movimientos en una única transacción:
    /// o se guarda todo, o no se guarda nada. Devuelve null si salió bien o el mensaje de error.
    /// </summary>
    private async Task<string?> RegistrarAsync(
        int idProducto,
        IEnumerable<(int idDeposito, int variacion)> ajustes,
        params Movimiento[] movimientos)
    {
        try
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();

            foreach (var (idDeposito, variacion) in ajustes)
                await AjustarStockAsync(idProducto, idDeposito, variacion);

            _context.Movimientos.AddRange(movimientos);
            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();
            return null;
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
        catch (DbUpdateException)
        {
            return "No se pudo registrar el movimiento. Verificá los datos e intentá nuevamente.";
        }
    }

    /// <summary>
    /// Suma (o resta, si es negativa) una cantidad al stock de un producto en un depósito.
    /// Crea el registro de stock si todavía no existe. No guarda: lo hace el método que la llama.
    /// </summary>
    private async Task AjustarStockAsync(int idProducto, int idDeposito, int variacion)
    {
        var stock = await _context.StockDepositos
            .FirstOrDefaultAsync(s => s.idProducto == idProducto && s.idDeposito == idDeposito);

        var cantidadActual = stock?.Cantidad ?? 0;
        if (cantidadActual + variacion < 0)
            throw new InvalidOperationException($"Stock insuficiente en el depósito. Disponible: {cantidadActual}.");

        if (stock == null)
        {
            _context.StockDepositos.Add(new StockDeposito
            {
                idProducto = idProducto,
                idDeposito = idDeposito,
                Cantidad = variacion
            });
            return;
        }

        stock.Cantidad = cantidadActual + variacion;
        stock.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Completa las listas desplegables según el tipo de operación.</summary>
    private async Task CargarListasAsync(OperacionStockViewModel model)
    {
        model.Productos = await ObtenerProductosAsync(model.idProducto);
        model.Depositos = await ObtenerDepositosAsync();

        switch (model)
        {
            case OperacionStockEntradaViewModel entrada:
                entrada.Proveedores = await ObtenerProveedoresAsync();
                break;
            case OperacionStockSalidaViewModel salida:
                salida.Clientes = await ObtenerClientesAsync();
                break;
        }
    }

    private Task<List<SelectListItem>> ObtenerProductosAsync(int? idSeleccionado = null, bool soloActivos = true) =>
        _context.Productos.AsNoTracking()
            .Where(p => !soloActivos || p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new SelectListItem { Value = p.id.ToString(), Text = p.Nombre, Selected = p.id == idSeleccionado })
            .ToListAsync();

    private Task<List<SelectListItem>> ObtenerDepositosAsync() =>
        _context.Depositos.AsNoTracking()
            .Where(d => d.Activo)
            .OrderBy(d => d.Nombre)
            .Select(d => new SelectListItem { Value = d.id.ToString(), Text = d.Nombre })
            .ToListAsync();

    private Task<List<SelectListItem>> ObtenerProveedoresAsync() =>
        _context.Proveedores.AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new SelectListItem { Value = p.id.ToString(), Text = p.Nombre })
            .ToListAsync();

    private Task<List<SelectListItem>> ObtenerClientesAsync() =>
        _context.Clientes.AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem { Value = c.id.ToString(), Text = c.Nombre })
            .ToListAsync();
}