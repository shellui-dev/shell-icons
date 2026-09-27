namespace ShellIcons.Maui;

/// <summary>Metadata for one icon, from its Lucide (or custom) JSON sidecar.</summary>
/// <param name="Name">The enum value.</param>
/// <param name="Id">Kebab-case catalog id, e.g. <c>chevron-right</c>.</param>
/// <param name="Source"><c>lucide</c> or <c>custom</c>.</param>
/// <param name="Tags">Search keywords.</param>
/// <param name="Categories">Lucide categories.</param>
/// <param name="Aliases">Former or alternative ids, e.g. <c>home</c> for <c>house</c>.</param>
public sealed record IconInfo(
    IconName Name,
    string Id,
    string Source,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Categories,
    IReadOnlyList<string> Aliases);

/// <summary>Maps ids, names and Lucide aliases to <see cref="IconName"/>, with tags and search.</summary>
public static partial class IconCatalog
{
    private static readonly Lazy<IReadOnlyList<IconInfo>> AllLazy = new(() =>
        Enum.GetValues<IconName>()
            .Where(n => n != IconName.None)
            .Select(n => Create(n)!)
            .ToArray());

    private static readonly Lazy<Dictionary<string, IconName>> Lookup = new(BuildLookup);

    /// <summary>Every icon, in catalog order.</summary>
    public static IReadOnlyList<IconInfo> All => AllLazy.Value;

    /// <summary>Metadata for <paramref name="name"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="name"/> is <see cref="IconName.None"/> or undefined.</exception>
    public static IconInfo Get(IconName name) =>
        Create(name) ?? throw new ArgumentOutOfRangeException(nameof(name), name, "Not an icon in the catalog.");

    /// <summary>Resolves <c>chevron-right</c>, <c>ChevronRight</c> or an alias like <c>home</c> (→ <c>House</c>). Case-insensitive.</summary>
    public static bool TryParse(string? value, out IconName name)
    {
        name = IconName.None;
        if (string.IsNullOrWhiteSpace(value)) return false;
        return Lookup.Value.TryGetValue(value.Trim(), out name);
    }

    /// <summary>Icons whose id, alias, tag or category contains <paramref name="query"/> (case-insensitive). Id matches first.</summary>
    public static IEnumerable<IconInfo> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return All;
        var q = query.Trim();

        return All
            .Select(i => (Info: i, Rank: Rank(i, q)))
            .Where(x => x.Rank >= 0)
            .OrderBy(x => x.Rank)
            .Select(x => x.Info);
    }

    private static int Rank(IconInfo info, string q)
    {
        if (info.Id.Contains(q, StringComparison.OrdinalIgnoreCase)) return 0;
        if (info.Aliases.Any(a => a.Contains(q, StringComparison.OrdinalIgnoreCase))) return 1;
        if (info.Tags.Any(t => t.Contains(q, StringComparison.OrdinalIgnoreCase))) return 2;
        if (info.Categories.Any(c => c.Contains(q, StringComparison.OrdinalIgnoreCase))) return 3;
        return -1;
    }

    private static Dictionary<string, IconName> BuildLookup()
    {
        var map = new Dictionary<string, IconName>(StringComparer.OrdinalIgnoreCase);

        // Canonical names first so an alias can never shadow a real icon id.
        foreach (var info in All)
        {
            map[info.Id] = info.Name;
            map[info.Name.ToString()] = info.Name;
        }
        foreach (var info in All)
        {
            foreach (var alias in info.Aliases)
                map.TryAdd(alias, info.Name);
        }

        return map;
    }
}
