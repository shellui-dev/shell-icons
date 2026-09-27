using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ShellIcons.Generator.Xaml;

internal sealed class IconMetadata
{
    public static readonly IconMetadata Empty = new(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

    public IconMetadata(IReadOnlyList<string> tags, IReadOnlyList<string> categories, IReadOnlyList<string> aliases)
    {
        Tags = tags;
        Categories = categories;
        Aliases = aliases;
    }

    public IReadOnlyList<string> Tags { get; }
    public IReadOnlyList<string> Categories { get; }
    public IReadOnlyList<string> Aliases { get; }
}

/* Targeted extraction instead of System.Text.Json, which Roslyn hosts don't reliably load
   inside an analyzer; the sidecar schema is flat enough for it. */
internal static class IconMetadataReader
{
    private static readonly Regex StringLiteral = new(@"""((?:\\.|[^""\\])*)""", RegexOptions.Compiled);

    public static IconMetadata Read(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return IconMetadata.Empty;
        return new IconMetadata(ReadArray(json, "tags"), ReadArray(json, "categories"), ReadArray(json, "aliases"));
    }

    private static IReadOnlyList<string> ReadArray(string json, string key)
    {
        var m = Regex.Match(json, "\"" + key + @"""\s*:\s*\[(?<body>[^\]]*)\]");
        if (!m.Success) return Array.Empty<string>();

        var body = m.Groups["body"].Value;
        // Newer Lucide writes aliases as objects ({ "name": "home", … }); take only "name".
        if (body.IndexOf('{') >= 0)
        {
            var names = new List<string>();
            foreach (Match o in Regex.Matches(body, @"""name""\s*:\s*""((?:\\.|[^""\\])*)"""))
                names.Add(Unescape(o.Groups[1].Value));
            return names;
        }

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
