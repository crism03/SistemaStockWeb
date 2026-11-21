using System.ComponentModel.DataAnnotations;

namespace SistemaStockWeb.Models.Enum;

public enum TipoMovimiento
{
    [Display(Name = "Entrada")]
    Entrada = 1,

    [Display(Name = "Salida")]
    Salida = 2,

    [Display(Name = "Transferencia (entrada)")]
    TransferenciaEntrada = 3,

    [Display(Name = "Transferencia (salida)")]
    TransferenciaSalida = 4
}