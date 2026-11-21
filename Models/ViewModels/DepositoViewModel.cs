using System.ComponentModel.DataAnnotations;

namespace SistemaStockWeb.Models.ViewModels;

public class DepositoViewModel
{
    public int id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Ubicación")]
    public string? Ubicacion { get; set; }

    public bool Activo { get; set; } = true;
}