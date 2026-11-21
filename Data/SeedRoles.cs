using Microsoft.AspNetCore.Identity;
using SistemaStockWeb.Models.Entities;

namespace SistemaStockWeb.Data;

/// <summary>
/// Crea los roles del sistema y el usuario administrador inicial si no existen.
/// </summary>
public static class SeedRoles
{
    private const string EmailAdmin = "admin@sistemastock.com";
    private const string PasswordAdmin = "Admin@12345";

    public static async Task InicializarAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = { "Administrador", "Encargado", "Operador", "Consulta" };
        foreach (var rol in roles)
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        if (await userManager.FindByEmailAsync(EmailAdmin) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = EmailAdmin,
                Email = EmailAdmin,
                EmailConfirmed = true,
                Nombre = "Administrador del Sistema",
                FechaRegistro = DateTime.UtcNow
            };

            var resultado = await userManager.CreateAsync(admin, PasswordAdmin);
            if (resultado.Succeeded)
                await userManager.AddToRoleAsync(admin, "Administrador");
        }
    }
}