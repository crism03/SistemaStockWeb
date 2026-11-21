using System.ComponentModel.DataAnnotations;

namespace SistemaStockWeb.Models.ViewModels.Cuenta;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresá un correo válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "¿Recordarme en este dispositivo?")]
    public bool RecordarSesion { get; set; }
}