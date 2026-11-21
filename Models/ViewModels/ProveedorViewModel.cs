using System.ComponentModel.DataAnnotations;

namespace SistemaStockWeb.Models.ViewModels;

public class ProveedorViewModel
{
    public int id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Nombre / Razón social")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    public string? Contacto { get; set; }

    [StringLength(30, ErrorMessage = "Máximo {1} caracteres.")]
    public string? Documento { get; set; }

    [Phone(ErrorMessage = "Ingresá un teléfono válido.")]
    [StringLength(30, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "Ingresá un correo válido.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "Máximo {1} caracteres.")]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [StringLength(300, ErrorMessage = "Máximo {1} caracteres.")]
    public string? Observaciones { get; set; }

    public bool Activo { get; set; } = true;
}