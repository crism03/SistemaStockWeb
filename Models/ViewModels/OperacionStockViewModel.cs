using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaStockWeb.Models.ViewModels.Movimiento;

/// <summary>
/// Datos comunes a las operaciones que modifican stock: entrada, salida y transferencia.
/// </summary>
public abstract class OperacionStockViewModel
{
    [Required(ErrorMessage = "Seleccioná un producto.")]
    [Display(Name = "Producto")]
    public int? idProducto { get; set; }
    public List<SelectListItem>? Productos { get; set; }

    [Required(ErrorMessage = "Seleccioná un depósito.")]
    [Display(Name = "Depósito")]
    public int? idDepositoOrigen { get; set; }
    public List<SelectListItem>? Depositos { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; } = 1;

    [StringLength(300, ErrorMessage = "Máximo {1} caracteres.")]
    public string? Observaciones { get; set; }
}