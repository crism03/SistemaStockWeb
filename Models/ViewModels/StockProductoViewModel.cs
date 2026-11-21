using SistemaStockWeb.Models.Enum;

namespace SistemaStockWeb.Models.ViewModels;

/// <summary>
/// Stock de un producto, desglosado por depósito.
/// </summary>
public class StockProductoViewModel
{
    public int id { get; set; }
    public string? Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? CategoriaNombre { get; set; }
    public UnidadMedida UnidadDeMedida { get; set; }
    public int StockMinimo { get; set; }
    public int Total { get; set; }
    public bool BajoMinimo { get; set; }

    /// <summary>Cantidad por depósito; la clave es el id del depósito.</summary>
    public Dictionary<int, int> CantidadPorDeposito { get; set; } = new();
}