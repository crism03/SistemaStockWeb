using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SistemaStockWeb.Models.Enum;

namespace SistemaStockWeb.Models.Entities;

/// <summary>
/// Registro histórico de cada entrada, salida o transferencia de mercadería.
/// </summary>
public class Movimiento : BaseEntity
{
    [Required]
    public TipoMovimiento Tipo { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }

    [Required]
    public DateTime Fecha { get; set; } = DateTime.Now;

    [StringLength(300)]
    public string? Observaciones { get; set; }

    [Required]
    [ForeignKey("Producto")]
    public int idProducto { get; set; }
    public Producto? Producto { get; set; }

    // Depósito donde se registra el movimiento.
    [ForeignKey("DepositoOrigen")]
    public int? idDepositoOrigen { get; set; }
    public Deposito? DepositoOrigen { get; set; }

    // Solo para transferencias.
    [ForeignKey("DepositoDestino")]
    public int? idDepositoDestino { get; set; }
    public Deposito? DepositoDestino { get; set; }

    // Solo para entradas.
    [ForeignKey("Proveedor")]
    public int? idProveedor { get; set; }
    public Proveedor? Proveedor { get; set; }

    // Solo para salidas.
    [ForeignKey("Cliente")]
    public int? idCliente { get; set; }
    public Cliente? Cliente { get; set; }

    [StringLength(100)]
    public string UsuarioRegistro { get; set; } = "sistema";
}