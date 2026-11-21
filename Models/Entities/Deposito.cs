using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaStockWeb.Models.Entities;

public class Deposito : BaseEntity
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Ubicacion { get; set; }

    public ICollection<StockDeposito> Stocks { get; set; } = new List<StockDeposito>();

    // Movimiento tiene dos relaciones con Deposito; InverseProperty indica a cuál corresponde cada colección.
    [InverseProperty("DepositoOrigen")]
    public ICollection<Movimiento> MovimientosOrigen { get; set; } = new List<Movimiento>();

    [InverseProperty("DepositoDestino")]
    public ICollection<Movimiento> MovimientosDestino { get; set; } = new List<Movimiento>();
}