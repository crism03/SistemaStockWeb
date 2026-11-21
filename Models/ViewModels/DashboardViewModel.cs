using SistemaStockWeb.Models.ViewModels.Movimiento;

namespace SistemaStockWeb.Models.ViewModels;

public class DashboardViewModel
{
    // Indicadores
    public decimal ValorTotalInventario { get; set; }
    public decimal ValorVentasHoy { get; set; }
    public int TotalProductosActivos { get; set; }
    public int TotalProveedoresActivos { get; set; }
    public int TotalClientesActivos { get; set; }
    public int TotalDepositosActivos { get; set; }
    public int MovimientosHoy { get; set; }
    public int TotalAlertas { get; set; }

    // Tablas
    public List<AlertaReposicionViewModel> AlertasReposicion { get; set; } = new();
    public List<MovimientoViewModel> UltimosMovimientos { get; set; } = new();

    // Gráficos
    public List<StockChartItemViewModel> TopMayorStock { get; set; } = new();
    public List<StockChartItemViewModel> TopMenorStock { get; set; } = new();
    public List<StockChartItemViewModel> StockPorDeposito { get; set; } = new();
}

public class AlertaReposicionViewModel
{
    public int idProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int StockTotal { get; set; }
    public int StockMinimo { get; set; }
    public bool SinStock => StockTotal <= 0;
    public string Estado => SinStock ? "Sin stock" : "Stock bajo";
}

/// <summary>Par etiqueta/valor para alimentar los gráficos.</summary>
public class StockChartItemViewModel
{
    public string Etiqueta { get; set; } = string.Empty;
    public int Valor { get; set; }
}