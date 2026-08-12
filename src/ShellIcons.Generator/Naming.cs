using System.Globalization;
using System.Text;

namespace ShellIcons.Generator;

/// <summary>Naming conversions between kebab-case (Lucide) and PascalCase (.NET).</summary>
internal static class Naming
{
    /// <summary>Converts kebab-case to PascalCase. <c>"chevron-right"</c> → <c>"ChevronRight"</c>.</summary>
    public static string KebabToPascal(string kebab)
    {
        if (string.IsNullOrEmpty(kebab)) return string.Empty;

        var sb = new StringBuilder(kebab.Length);
        var nextUpper = true;

        foreach (var c in kebab)
        {
            if (c == '-' || c == '_')
            {
                nextUpper = true;
                continue;
            }

            sb.Append(nextUpper ? char.ToUpper(c, CultureInfo.InvariantCulture) : c);
            nextUpper = false;
        }

        return sb.ToString();
    }
}
