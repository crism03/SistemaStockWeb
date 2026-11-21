using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SistemaStockWeb.Data;
using SistemaStockWeb.Extensions;
using SistemaStockWeb.Models.Entities;
using SistemaStockWeb.Models.Enum;

namespace SistemaStockWeb.Controllers;

[Authorize]
public class ReporteController : Controller
{
    private static readonly string[] Encabezados =
        { "ID", "Fecha", "Tipo", "Producto", "Depósito origen", "Depósito destino", "Cantidad", "Usuario" };

    private readonly AppDbContext _context;

    public ReporteController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Reporte
    public IActionResult Index()
    {
        return View();
    }

    // GET: Reporte/DescargarExcel
    public async Task<IActionResult> DescargarExcel(DateTime? desde, DateTime? hasta, TipoMovimiento? tipo)
    {
        var movimientos = await FiltrarMovimientosAsync(desde, hasta, tipo);

        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Movimientos");

        // Título
        var titulo = hoja.Cell(1, 1);
        titulo.Value = "Reporte de Movimientos de Stock";
        titulo.Style.Font.Bold = true;
        titulo.Style.Font.FontSize = 16;
        hoja.Range(1, 1, 1, Encabezados.Length).Merge();

        // Encabezados de columnas
        for (var i = 0; i < Encabezados.Length; i++)
        {
            var celda = hoja.Cell(2, i + 1);
            celda.Value = Encabezados[i];
            celda.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E75B6");
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Font.Bold = true;
        }

        // Filas de datos
        var fila = 3;
        foreach (var m in movimientos)
        {
            hoja.Cell(fila, 1).Value = m.id;
            hoja.Cell(fila, 2).Value = m.Fecha;
            hoja.Cell(fila, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            hoja.Cell(fila, 3).Value = m.Tipo.ObtenerNombre();
            hoja.Cell(fila, 4).Value = m.Producto?.Nombre ?? string.Empty;
            hoja.Cell(fila, 5).Value = m.DepositoOrigen?.Nombre ?? string.Empty;
            hoja.Cell(fila, 6).Value = m.DepositoDestino?.Nombre ?? string.Empty;
            hoja.Cell(fila, 7).Value = m.Cantidad;
            hoja.Cell(fila, 8).Value = m.UsuarioRegistro;

            hoja.Range(fila, 1, fila, Encabezados.Length).Style.Fill.BackgroundColor = ColorFila(m.Tipo);
            fila++;
        }

        // Autoajustar columnas (desde la fila de encabezados, para que el título no ensanche la primera)
        hoja.Columns().AdjustToContents(2);

        using var stream = new MemoryStream();
        libro.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"movimientos_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // GET: Reporte/DescargarPdf
    public async Task<IActionResult> DescargarPdf(DateTime? desde, DateTime? hasta, TipoMovimiento? tipo)
    {
        var movimientos = await FiltrarMovimientosAsync(desde, hasta, tipo);

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4.Landscape());
                pagina.Margin(30);
                pagina.DefaultTextStyle(x => x.FontSize(9));

                pagina.Header().Column(columna =>
                {
                    columna.Item().Text("Reporte de Movimientos de Stock")
                        .Bold().FontSize(18).FontColor("#1F4E79");
                    columna.Item().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontColor(Colors.Grey.Darken1);
                });

                pagina.Content().PaddingTop(10).Table(tabla =>
                {
                    tabla.ColumnsDefinition(columnas =>
                    {
                        columnas.ConstantColumn(35);   // ID
                        columnas.ConstantColumn(85);   // Fecha
                        columnas.ConstantColumn(110);  // Tipo
                        columnas.RelativeColumn(3);    // Producto
                        columnas.RelativeColumn(2);    // Depósito origen
                        columnas.RelativeColumn(2);    // Depósito destino
                        columnas.ConstantColumn(45);   // Cantidad
                        columnas.RelativeColumn(2);    // Usuario
                    });

                    tabla.Header(encabezado =>
                    {
                        foreach (var titulo in Encabezados)
                            encabezado.Cell().Background("#2E75B6").Padding(5)
                                .Text(titulo).Bold().FontColor(Colors.White);
                    });

                    var alternar = false;
                    foreach (var m in movimientos)
                    {
                        var fondo = alternar ? "#F2F2F2" : "#FFFFFF";
                        alternar = !alternar;

                        var valores = new[]
                        {
                            m.id.ToString(),
                            m.Fecha.ToString("dd/MM/yy HH:mm"),
                            m.Tipo.ObtenerNombre(),
                            m.Producto?.Nombre ?? string.Empty,
                            m.DepositoOrigen?.Nombre ?? "-",
                            m.DepositoDestino?.Nombre ?? "-",
                            m.Cantidad.ToString(),
                            m.UsuarioRegistro
                        };

                        foreach (var valor in valores)
                            tabla.Cell().Background(fondo).Padding(4).Text(valor);
                    }
                });

                pagina.Footer().AlignRight().Text(texto =>
                {
                    texto.Span("Página ");
                    texto.CurrentPageNumber();
                    texto.Span(" de ");
                    texto.TotalPages();
                });
            });
        });

        return File(documento.GeneratePdf(), "application/pdf", $"movimientos_{DateTime.Now:yyyyMMdd}.pdf");
    }

    /// <summary>Movimientos del período y tipo indicados; el límite "hasta" incluye todo ese día.</summary>
    private async Task<List<Movimiento>> FiltrarMovimientosAsync(DateTime? desde, DateTime? hasta, TipoMovimiento? tipo)
    {
        var consulta = _context.Movimientos
            .AsNoTracking()
            .Include(m => m.Producto)
            .Include(m => m.DepositoOrigen)
            .Include(m => m.DepositoDestino)
            .AsQueryable();

        if (desde.HasValue)
        {
            var inicio = desde.Value.Date;
            consulta = consulta.Where(m => m.Fecha >= inicio);
        }

        if (hasta.HasValue)
        {
            var limite = hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(m => m.Fecha < limite);
        }

        if (tipo.HasValue)
            consulta = consulta.Where(m => m.Tipo == tipo.Value);

        return await consulta.OrderByDescending(m => m.Fecha).ThenByDescending(m => m.id).ToListAsync();
    }

    private static XLColor ColorFila(TipoMovimiento tipo) => tipo switch
    {
        TipoMovimiento.Entrada => XLColor.FromHtml("#D6F0DC"),
        TipoMovimiento.Salida => XLColor.FromHtml("#FFEBEE"),
        _ => XLColor.FromHtml("#FFF9C4")
    };
}