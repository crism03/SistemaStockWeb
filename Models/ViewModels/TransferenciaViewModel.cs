using System.ComponentModel.DataAnnotations;

namespace SistemaStockWeb.Models.ViewModels.Movimiento;

public class TransferenciaViewModel : OperacionStockViewModel
{
    [Required(ErrorMessage = "Seleccioná el depósito de destino.")]
    [Display(Name = "Depósito destino")]
    public int? idDepositoDestino { get; set; }
}