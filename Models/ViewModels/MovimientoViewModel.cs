using System.ComponentModel.DataAnnotations;
using SistemaStockWeb.Models.Enum;

namespace SistemaStockWeb.Models.ViewModels.Movimiento;


public class MovimientoViewModel
{
    public int id { get; set; }
    public TipoMovimiento Tipo { get; set; }
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }
    public string? Observaciones { get; set; }

    [Display(Name = "Producto")]
    public string? ProductoNombre { get; set; }

    [Display(Name = "Depósito origen")]
    public string? DepositoOrigenNombre { get; set; }

    [Display(Name = "Depósito destino")]
    public string? DepositoDestinoNombre { get; set; }

    [Display(Name = "Proveedor")]
    public string? ProveedorNombre { get; set; }

    [Display(Name = "Cliente")]
    public string? ClienteNombre { get; set; }

    [Display(Name = "Usuario")]
    public string UsuarioRegistro { get; set; } = string.Empty;
}