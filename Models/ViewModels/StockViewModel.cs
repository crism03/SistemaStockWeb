namespace SistemaStockWeb.Models.ViewModels;

public class StockViewModel
{
    public List<DepositoViewModel> Depositos { get; set; } = new();
    public List<StockProductoViewModel> Productos { get; set; } = new();
}