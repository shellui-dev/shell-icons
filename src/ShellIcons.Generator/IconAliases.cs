using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellIcons.Generator;

/// <summary>Another name for an icon, usually the name it had before Lucide renamed it.</summary>
internal sealed class IconAlias
{
    public IconAlias(string kebab, string pascal, string targetKebab, string targetPascal, bool deprecated)
    {
        Kebab = kebab;
        Pascal = pascal;
        TargetKebab = targetKebab;
        TargetPascal = targetPascal;
        Deprecated = deprecated;
    }

    public string Kebab { get; }        // "trash-2"
    public string Pascal { get; }       // "Trash2"
    public string TargetKebab { get; }  // "trash"
    public string TargetPascal { get; } // "Trash"
    public bool Deprecated { get; }
}

/// <summary>An icon's own names plus the aliases its metadata lists.</summary>
internal sealed class AliasSource
{
    public AliasSource(string kebab, string pascal, IReadOnlyList<string> aliases, IReadOnlyCollection<string> deprecated)
    {
        Kebab = kebab;
        Pascal = pascal;
        Aliases = aliases;
        Deprecated = deprecated;
    }

    public string Kebab { get; }
    public string Pascal { get; }
    public IReadOnlyList<string> Aliases { get; }
    public IReadOnlyCollection<string> Deprecated { get; }
}

internal static class IconAliases
{
    /* Aliases that get their own C# names. Icon names win, then the first alias in id order. Names are
       compared ignoring case, because generated file names are, and because Lucide lists spelling variants
       (arrow-down-01 for arrow-down-0-1) that would produce the same identifier. */
    public static IReadOnlyList<IconAlias> Resolve(IEnumerable<AliasSource> icons, IEnumerable<string>? reservedNames = null)
    {
        var ordered = icons.OrderBy(i => i.Kebab, StringComparer.Ordinal).ToArray();
        var taken = new HashSet<string>(ordered.Select(i => i.Pascal), StringComparer.OrdinalIgnoreCase);
        var takenIds = new HashSet<string>(ordered.Select(i => i.Kebab), StringComparer.OrdinalIgnoreCase);
        if (reservedNames is not null) taken.UnionWith(reservedNames);

        var result = new List<IconAlias>();
        foreach (var icon in ordered)
        {
            var deprecated = new HashSet<string>(icon.Deprecated, StringComparer.Ordinal);
            foreach (var alias in icon.Aliases)
            {
                var pascal = Naming.KebabToPascal(alias);
                if (pascal.Length == 0 || !char.IsLetter(pascal[0])) continue;
                if (takenIds.Contains(alias) || !taken.Add(pascal)) continue;

                result.Add(new IconAlias(alias, pascal, icon.Kebab, icon.Pascal, deprecated.Contains(alias)));
            }
        }

        return result;
    }

    /// <summary>The <c>[Obsolete]</c> message for a renamed icon: <c>Lucide renamed 'trash-2' to 'trash'. Use Trash.</c></summary>
    public static string ObsoleteMessage(IconAlias alias, string replacement) =>
        $"Lucide renamed '{alias.Kebab}' to '{alias.TargetKebab}'. Use {replacement}.";
}
