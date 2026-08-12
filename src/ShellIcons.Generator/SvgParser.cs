using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ShellIcons.Generator;

/// <summary>
/// Extracts the inner shape markup from a Lucide-format SVG (strips the outer &lt;svg&gt;
/// wrapper and collapses whitespace between shape elements). Regex-based because Lucide's
/// SVG format is rigid and consistent — no need to spin up an XML parser.
/// </summary>
internal static class SvgParser
{
    private static readonly Regex SvgOpenTag = new(
        @"<svg\b[^>]*>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex SvgCloseTag = new(
        @"</svg\s*>",
        RegexOptions.Compiled);

    private static readonly Regex BetweenTags = new(
        @">\s+<",
        RegexOptions.Compiled);

    /// <summary>Extracts the inner shape markup as a single line, e.g. <c>&lt;path d="…"/&gt;&lt;circle …/&gt;</c>.</summary>
    public static string ExtractInner(string svg)
    {
        if (string.IsNullOrEmpty(svg)) return string.Empty;

        var openMatch = SvgOpenTag.Match(svg);
        if (!openMatch.Success) return string.Empty;

        var innerStart = openMatch.Index + openMatch.Length;

        var closeMatch = SvgCloseTag.Match(svg, innerStart);
        var innerEnd = closeMatch.Success ? closeMatch.Index : svg.Length;

        var inner = svg.Substring(innerStart, innerEnd - innerStart).Trim();

        inner = BetweenTags.Replace(inner, "><");

        return NormalizeWhitespace(inner);
    }

    private static string NormalizeWhitespace(string s)
    {
        var sb = new StringBuilder(s.Length);
        var previousWasSpace = false;
        var insideTag = false;

        foreach (var c in s)
        {
            if (c == '<') insideTag = true;
            else if (c == '>') insideTag = false;

            if (insideTag && (c == '\n' || c == '\r' || c == '\t'))
            {
                if (!previousWasSpace)
                {
                    sb.Append(' ');
                    previousWasSpace = true;
                }
                continue;
            }

            sb.Append(c);
            previousWasSpace = c == ' ';
        }

        return sb.ToString();
    }
}
