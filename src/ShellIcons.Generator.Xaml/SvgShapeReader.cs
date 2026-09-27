using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ShellIcons.Generator.Xaml;

internal sealed class XamlIconShapes
{
    public XamlIconShapes(string strokeData, string? fillData, IReadOnlyList<string> unsupported, IReadOnlyList<string> offCanvas)
    {
        StrokeData = strokeData;
        FillData = fillData;
        Unsupported = unsupported;
        OffCanvas = offCanvas;
    }

    // All stroke-only shapes, merged into one path.
    public string StrokeData { get; }

    // fill="currentColor" shapes, drawn by a second filled + stroked path.
    public string? FillData { get; }

    // Elements/attributes the XAML targets can't draw (SHELLICONS002).
    public IReadOnlyList<string> Unsupported { get; }

    // Shapes entirely outside the viewBox: browsers clip them, a native Path wouldn't (SHELLICONS005).
    public IReadOnlyList<string> OffCanvas { get; }
}

// Regex-based like SvgParser: the contract is flat, so anything outside it is reported, not guessed at.
internal static class SvgShapeReader
{
    private static readonly Regex Element = new(
        @"<(?<name>[a-zA-Z][\w:-]*)(?<attrs>[^>]*?)/?>",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex Attribute = new(
        @"(?<name>[a-zA-Z_:][\w:.-]*)\s*=\s*""(?<value>[^""]*)""",
        RegexOptions.Compiled);

    private static readonly HashSet<string> Ignored = new(StringComparer.Ordinal) { "svg", "title", "desc" };

    public static XamlIconShapes Read(string svg)
    {
        var stroke = new StringBuilder();
        var fill = new StringBuilder();
        var unsupported = new List<string>();
        var offCanvas = new List<string>();

        foreach (Match m in Element.Matches(svg ?? string.Empty))
        {
            var name = m.Groups["name"].Value;
            if (Ignored.Contains(name)) continue;

            var attrs = ParseAttributes(m.Groups["attrs"].Value);
            if (attrs.ContainsKey("transform")) unsupported.Add($"<{name} transform>");

            string? data;
            try
            {
                data = ToPath(name, attrs);
            }
            catch (FormatException ex)
            {
                unsupported.Add($"<{name}> ({ex.Message})");
                continue;
            }

            if (data is null)
            {
                unsupported.Add($"<{name}>");
                continue;
            }
            if (data.Length == 0) continue;

            if (IsOffCanvas(data))
            {
                offCanvas.Add($"<{name}> {data}");
                continue;
            }

            var target = IsFilled(attrs) ? fill : stroke;
            if (target.Length > 0) target.Append(' ');
            target.Append(data);
        }

        return new XamlIconShapes(stroke.ToString(), fill.Length == 0 ? null : fill.ToString(), unsupported, offCanvas);
    }

    /* Conservative: control points count, arcs count endpoint ± radius, and the box grows by half
       of Lucide's 2-unit stroke, so anything that could leave a visible mark is kept. */
    internal static bool IsOffCanvas(string normalized)
    {
        const double min = -1, max = 25;
        double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;

        void Add(double x, double y, double pad)
        {
            minX = Math.Min(minX, x - pad); maxX = Math.Max(maxX, x + pad);
            minY = Math.Min(minY, y - pad); maxY = Math.Max(maxY, y + pad);
        }

        var t = normalized.Split(' ');
        for (var i = 0; i < t.Length;)
        {
            switch (t[i++])
            {
                case "M": case "L": Add(D(t[i]), D(t[i + 1]), 0); i += 2; break;
                case "Q": Add(D(t[i]), D(t[i + 1]), 0); Add(D(t[i + 2]), D(t[i + 3]), 0); i += 4; break;
                case "C": Add(D(t[i]), D(t[i + 1]), 0); Add(D(t[i + 2]), D(t[i + 3]), 0); Add(D(t[i + 4]), D(t[i + 5]), 0); i += 6; break;
                case "A": Add(D(t[i + 5]), D(t[i + 6]), Math.Max(D(t[i]), D(t[i + 1]))); i += 7; break;
                default: break;   // Z
            }
        }

        if (minX > maxX) return false;   // no points at all: nothing to judge
        return maxX < min || minX > max || maxY < min || minY > max;
    }

    private static double D(string s) => double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);

    // null = unsupported element; "" = supported but draws nothing (e.g. zero radius).
    private static string? ToPath(string name, Dictionary<string, string> a)
    {
        switch (name)
        {
            case "path":
                return a.TryGetValue("d", out var d) ? PathNormalizer.NormalizePathData(d) : string.Empty;
            case "circle":
            {
                var r = Num(a, "r");
                return r > 0 ? PathNormalizer.Circle(Num(a, "cx"), Num(a, "cy"), r) : string.Empty;
            }
            case "ellipse":
            {
                var rx = Num(a, "rx"); var ry = Num(a, "ry");
                return rx > 0 && ry > 0 ? PathNormalizer.Ellipse(Num(a, "cx"), Num(a, "cy"), rx, ry) : string.Empty;
            }
            case "rect":
            {
                var w = Num(a, "width"); var h = Num(a, "height");
                if (w <= 0 || h <= 0) return string.Empty;
                return PathNormalizer.Rect(Num(a, "x"), Num(a, "y"), w, h, NumOrNull(a, "rx"), NumOrNull(a, "ry"));
            }
            case "line":
                return PathNormalizer.Line(Num(a, "x1"), Num(a, "y1"), Num(a, "x2"), Num(a, "y2"));
            case "polyline":
                return a.TryGetValue("points", out var pl) ? PathNormalizer.Poly(pl, close: false) : string.Empty;
            case "polygon":
                return a.TryGetValue("points", out var pg) ? PathNormalizer.Poly(pg, close: true) : string.Empty;
            default:
                return null;
        }
    }

    // Lucide shapes are stroke-only unless they opt into a fill; `fill="none"` is the root default.
    private static bool IsFilled(Dictionary<string, string> a) =>
        a.TryGetValue("fill", out var f) && f.Length > 0 && !string.Equals(f, "none", StringComparison.OrdinalIgnoreCase);

    private static Dictionary<string, string> ParseAttributes(string raw)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match m in Attribute.Matches(raw))
            result[m.Groups["name"].Value] = m.Groups["value"].Value;
        return result;
    }

    private static double Num(Dictionary<string, string> a, string key) => NumOrNull(a, key) ?? 0;

    private static double? NumOrNull(Dictionary<string, string> a, string key)
    {
        if (!a.TryGetValue(key, out var raw)) return null;
        raw = raw.Trim();
        if (raw.EndsWith("px", StringComparison.Ordinal)) raw = raw.Substring(0, raw.Length - 2);
        if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            throw new FormatException($"{key}=\"{a[key]}\" is not a number");
        return v;
    }
}
