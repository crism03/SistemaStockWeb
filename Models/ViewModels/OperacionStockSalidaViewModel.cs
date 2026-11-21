using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SistemaStockWeb.Models.ViewModels.Movimiento;

public class OperacionStockSalidaViewModel : OperacionStockViewModel
{
    [Required(ErrorMessage = "Seleccioná un cliente.")]
    [Display(Name = "Cliente")]
    public int? idCliente { get; set; }
    public List<SelectListItem>? Clientes { get; set; }
}