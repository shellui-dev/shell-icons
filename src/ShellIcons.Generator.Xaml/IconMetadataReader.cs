using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ShellIcons.Generator.Xaml;

internal sealed class IconMetadata
{
    public static readonly IconMetadata Empty = new(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

    public IconMetadata(IReadOnlyList<string> tags, IReadOnlyList<string> categories, IReadOnlyList<string> aliases,
        IReadOnlyCollection<string>? deprecatedAliases = null)
    {
        Tags = tags;
        Categories = categories;
        Aliases = aliases;
        DeprecatedAliases = deprecatedAliases ?? Array.Empty<string>();
    }

    public IReadOnlyList<string> Tags { get; }
    public IReadOnlyList<string> Categories { get; }
    public IReadOnlyList<string> Aliases { get; }

    // Aliases Lucide marks "deprecated": old names of a renamed icon.
    public IReadOnlyCollection<string> DeprecatedAliases { get; }
}

/* Targeted extraction instead of System.Text.Json, which Roslyn hosts don't reliably load
   inside an analyzer; the sidecar schema is flat enough for it. */
internal static class IconMetadataReader
{
    private const string ExtendsPrefix = "$extends:";
    private const string GroupPrefix = "$group:";

    private static readonly Regex StringLiteral = new(@"""((?:\\.|[^""\\])*)""", RegexOptions.Compiled);
    private static readonly Regex AliasObject = new(@"\{[^{}]*\}", RegexOptions.Compiled);
    private static readonly Regex NameField = new(@"""name""\s*:\s*""((?:\\.|[^""\\])*)""", RegexOptions.Compiled);
    private static readonly Regex DeprecatedTrue = new(@"""deprecated""\s*:\s*true", RegexOptions.Compiled);

    public static IconMetadata Read(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return IconMetadata.Empty;

        var deprecated = new List<string>();
        var aliases = ReadAliases(json, deprecated);
        return new IconMetadata(ReadArray(json, "tags"), ReadArray(json, "categories"), aliases, deprecated);
    }

    /* Sidecars by icon id from catalog/lucide and catalog/custom (a custom one replaces Lucide's), with the tag groups
       from catalog/lucide/groups/ applied. Keys of `files` are paths, values the JSON text. */
    public static Dictionary<string, IconMetadata> ReadCatalog(IEnumerable<KeyValuePair<string, string>> files)
    {
        var icons = new Dictionary<string, IconMetadata>(StringComparer.Ordinal);
        var groups = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

        foreach (var file in files)
        {
            var path = file.Key.Replace('\\', '/');
            if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) continue;

            var slash = path.LastIndexOf('/');
            var id = path.Substring(slash + 1, path.Length - slash - 1 - ".json".Length);
            var meta = Read(file.Value);

            if (path.Contains("/catalog/lucide/groups/")) groups[id] = meta.Tags;
            else if (path.Contains("/catalog/custom/") || !icons.ContainsKey(id)) icons[id] = meta;
        }

        return ResolveDirectives(icons, groups);
    }

    /* Lucide 1.x metadata shares tags: "$extends:alarm-clock" copies another icon's tags (or categories)
       and "$group:check" copies a tag group from groups/. Expands both; unknown targets are dropped. */
    public static Dictionary<string, IconMetadata> ResolveDirectives(
        IReadOnlyDictionary<string, IconMetadata> icons,
        IReadOnlyDictionary<string, IReadOnlyList<string>> groups)
    {
        var resolved = new Dictionary<string, IconMetadata>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);

        IconMetadata Resolve(string id)
        {
            if (resolved.TryGetValue(id, out var done)) return done;
            if (!icons.TryGetValue(id, out var meta)) return IconMetadata.Empty;
            if (!visiting.Add(id)) return meta; // cycle: use the raw values

            var tags = Expand(meta.Tags, t => Resolve(t).Tags, groups);
            var categories = Expand(meta.Categories, t => Resolve(t).Categories, groups);
            visiting.Remove(id);

            var result = new IconMetadata(tags, categories, meta.Aliases, meta.DeprecatedAliases);
            resolved[id] = result;
            return result;
        }

        foreach (var id in icons.Keys) Resolve(id);
        return resolved;
    }

    private static IReadOnlyList<string> Expand(
        IReadOnlyList<string> values,
        Func<string, IReadOnlyList<string>> extends,
        IReadOnlyDictionary<string, IReadOnlyList<string>> groups)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<string>(values.Count);

        foreach (var value in values)
        {
            IEnumerable<string> expanded;
            if (value.StartsWith(ExtendsPrefix, StringComparison.Ordinal))
                expanded = extends(value.Substring(ExtendsPrefix.Length));
            else if (value.StartsWith(GroupPrefix, StringComparison.Ordinal))
                expanded = groups.TryGetValue(value.Substring(GroupPrefix.Length), out var g) ? g : Array.Empty<string>();
            else
                expanded = new[] { value };

            foreach (var v in expanded)
            {
                if (seen.Add(v)) result.Add(v);
            }
        }

        return result;
    }

    private static IReadOnlyList<string> ReadAliases(string json, List<string> deprecated)
    {
        var body = ArrayBody(json, "aliases");
        if (body is null) return Array.Empty<string>();

        // Newer Lucide writes aliases as objects ({ "name": "home", "deprecated": true, … }).
        if (body.IndexOf('{') < 0) return ReadStrings(body);

        var names = new List<string>();
        foreach (Match o in AliasObject.Matches(body))
        {
            var name = NameField.Match(o.Value);
            if (!name.Success) continue;

            var value = Unescape(name.Groups[1].Value);
            names.Add(value);
            if (DeprecatedTrue.IsMatch(o.Value)) deprecated.Add(value);
        }
        return names;
    }

    private static IReadOnlyList<string> ReadArray(string json, string key)
    {
        var body = ArrayBody(json, key);
        return body is null ? Array.Empty<string>() : ReadStrings(body);
    }

    private static string? ArrayBody(string json, string key)
    {
        var m = Regex.Match(json, "\"" + key + @"""\s*:\s*\[(?<body>[^\]]*)\]");
        return m.Success ? m.Groups["body"].Value : null;
    }

    private static IReadOnlyList<string> ReadStrings(string body)
    {
        var result = new List<string>();
        foreach (Match s in StringLiteral.Matches(body))
            result.Add(Unescape(s.Groups[1].Value));
        return result;
    }

    private static string Unescape(string s)
    {
        if (s.IndexOf('\\') < 0) return s;
        var sb = new StringBuilder(s.Length);
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] != '\\' || i + 1 >= s.Length) { sb.Append(s[i]); continue; }
            var next = s[++i];
            switch (next)
            {
                case 'n': sb.Append('\n'); break;
                case 't': sb.Append('\t'); break;
                case 'u' when i + 4 < s.Length:
                    sb.Append((char)Convert.ToInt32(s.Substring(i + 1, 4), 16));
                    i += 4;
                    break;
                default: sb.Append(next); break;
            }
        }
        return sb.ToString();
    }
}
