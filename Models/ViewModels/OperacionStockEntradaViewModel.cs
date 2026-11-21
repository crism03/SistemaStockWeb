using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaStockWeb.Models.ViewModels.Movimiento;

public class OperacionStockEntradaViewModel : OperacionStockViewModel
{
    [Required(ErrorMessage = "Seleccioná un proveedor.")]
    [Display(Name = "Proveedor")]
    public int? idProveedor { get; set; }
    public List<SelectListItem>? Proveedores { get; set; }
}