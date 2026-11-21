using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaStockWeb.Models.Enum;

namespace SistemaStockWeb.Models.ViewModels;

public class ProductoViewModel
{
    public int id { get; set; }

    [StringLength(50, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Código")]
    public string? Codigo { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre del producto")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "Seleccioná una categoría.")]
    [Display(Name = "Categoría")]
    public int? idCategoria { get; set; }

    public string? CategoriaNombre { get; set; }

    // Opciones del combo de categorías (solo se usa para dibujar el formulario).
    public List<SelectListItem>? Categorias { get; set; }

    [Display(Name = "Unidad de medida")]
    public UnidadMedida UnidadDeMedida { get; set; }

    [Range(0, 1000000, ErrorMessage = "El precio debe estar entre {1} y {2}.")]
    [Display(Name = "Precio de costo")]
    public decimal PrecioCosto { get; set; }

    [Range(0, 1000000, ErrorMessage = "El precio debe estar entre {1} y {2}.")]
    [Display(Name = "Precio de venta")]
    public decimal PrecioVenta { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo debe ser un número positivo.")]
    [Display(Name = "Stock mínimo")]
    public int StockMinimo { get; set; } = 5;

    public bool Activo { get; set; } = true;

    [Display(Name = "Fecha de creación")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Fecha de actualización")]
    public DateTime? UpdatedAt { get; set; }
}