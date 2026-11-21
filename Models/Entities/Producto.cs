using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemaStockWeb.Models.Enum;

namespace SistemaStockWeb.Models.Entities;

public class Producto : BaseEntity
{
    [StringLength(50)]
    public string? Codigo { get; set; }

    [Required, StringLength(100)]
    public required string Nombre { get; set; }

    [StringLength(300)]
    public string? Descripcion { get; set; }

    [Required]
    [ForeignKey("Categoria")]
    public int idCategoria { get; set; }
    public Categoria Categoria { get; set; } = null!;

    [Required]
    public UnidadMedida UnidadDeMedida { get; set; }

    [Required]
    [Range(0, 1000000)]
    [DataType(DataType.Currency)]
    public decimal PrecioCosto { get; set; }

    [Required]
    [Range(0, 1000000)]
    [DataType(DataType.Currency)]
    public decimal PrecioVenta { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int StockMinimo { get; set; } = 5;

    public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();
    public ICollection<StockDeposito> Stocks { get; set; } = new List<StockDeposito>();

    /// <summary>Suma del stock en todos los depósitos (no se guarda en la base).</summary>
    [NotMapped]
    public int StockTotal => Stocks.Sum(s => s.Cantidad);

    /// <summary>Indica si el stock total llegó al mínimo o quedó por debajo (no se guarda en la base).</summary>
    [NotMapped]
    public bool BajoStockMinimo => StockTotal <= StockMinimo;
}