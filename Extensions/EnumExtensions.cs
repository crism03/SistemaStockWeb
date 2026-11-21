using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SistemaStockWeb.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// Devuelve el texto del atributo [Display(Name = "...")] del valor del enum;
    /// si no lo tiene, devuelve el nombre del valor.
    /// </summary>
    public static string ObtenerNombre(this System.Enum valor)
    {
        var miembro = valor.GetType().GetMember(valor.ToString()).FirstOrDefault();
        return miembro?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? valor.ToString();
    }
}