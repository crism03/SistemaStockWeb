using System.ComponentModel.DataAnnotations;

namespace SistemaStockWeb.Models.ViewModels.Usuario;

public class EditarUsuarioViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "Máximo {1} caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresá un correo válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccioná un rol.")]
    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }
}