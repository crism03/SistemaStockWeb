using System.ComponentModel.DataAnnotations;

namespace SistemaStockWeb.Models.ViewModels;

public class CategoriaViewModel
{
    public int id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre no puede superar los {1} caracteres.")]
    [Display(Name = "Nombre de la categoría")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción no puede superar los {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    [Display(Name = "Fecha de creación")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Fecha de actualización")]
    public DateTime? UpdatedAt { get; set; }

    [Display(Name = "Cantidad de productos")]
    public int CantidadProductos { get; set; }
}