using Microsoft.AspNetCore.Identity;

namespace SistemaStockWeb.Services;

/// <summary>
/// Traduce al español los mensajes de error que genera ASP.NET Core Identity.
/// </summary>
public class EspanolIdentityErrorDescriber : IdentityErrorDescriber
{
    private static IdentityError Crear(string codigo, string descripcion) =>
        new() { Code = codigo, Description = descripcion };

    public override IdentityError DefaultError() =>
        Crear(nameof(DefaultError), "Ha ocurrido un error desconocido.");

    public override IdentityError ConcurrencyFailure() =>
        Crear(nameof(ConcurrencyFailure), "Error de concurrencia: el registro fue modificado por otro usuario.");

    public override IdentityError PasswordMismatch() =>
        Crear(nameof(PasswordMismatch), "Contraseña incorrecta.");

    public override IdentityError InvalidToken() =>
        Crear(nameof(InvalidToken), "Token inválido.");

    public override IdentityError LoginAlreadyAssociated() =>
        Crear(nameof(LoginAlreadyAssociated), "Ya existe un usuario con este inicio de sesión.");

    public override IdentityError InvalidUserName(string? userName) =>
        Crear(nameof(InvalidUserName), $"El nombre de usuario '{userName}' es inválido.");

    public override IdentityError InvalidEmail(string? email) =>
        Crear(nameof(InvalidEmail), $"El correo electrónico '{email}' es inválido.");

    public override IdentityError DuplicateUserName(string userName) =>
        Crear(nameof(DuplicateUserName), $"El nombre de usuario '{userName}' ya está en uso.");

    public override IdentityError DuplicateEmail(string email) =>
        Crear(nameof(DuplicateEmail), $"El correo electrónico '{email}' ya está registrado.");

    public override IdentityError PasswordRequiresLower() =>
        Crear(nameof(PasswordRequiresLower), "La contraseña debe tener al menos una letra minúscula ('a'-'z').");

    public override IdentityError PasswordRequiresUpper() =>
        Crear(nameof(PasswordRequiresUpper), "La contraseña debe tener al menos una letra mayúscula ('A'-'Z').");

    public override IdentityError PasswordRequiresDigit() =>
        Crear(nameof(PasswordRequiresDigit), "La contraseña debe tener al menos un dígito ('0'-'9').");

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Crear(nameof(PasswordRequiresNonAlphanumeric), "La contraseña debe tener al menos un carácter no alfanumérico.");

    public override IdentityError PasswordTooShort(int length) =>
        Crear(nameof(PasswordTooShort), $"La contraseña debe tener al menos {length} caracteres.");
}