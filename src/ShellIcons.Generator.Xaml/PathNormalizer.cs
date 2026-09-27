using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ShellIcons.Generator.Xaml;

/* SVG shapes → one path with absolute commands only, space-separated:
   M x y | L x y | C x1 y1 x2 y2 x y | Q x1 y1 x y | A rx ry rot large sweep x y | Z
   Valid for MAUI's PathGeometryConverter and Avalonia's StreamGeometry.Parse. */
internal static class PathNormalizer
{
    // Relative → absolute, H/V → L, S → C, T → Q.
    public static string NormalizePathData(string d)
    {
        var sb = new StringBuilder(d.Length + 16);
        var reader = new Reader(d);

        double cx = 0, cy = 0;          // current point
        double sx = 0, sy = 0;          // current sub-path start
        double lcx = 0, lcy = 0;        // last control point (for S/T reflection)
        var lastCmd = '\0';
        var cmd = '\0';

        while (true)
        {
            reader.SkipSeparators();
            if (reader.AtEnd) break;

            if (reader.PeekIsCommand())
            {
                cmd = reader.ReadCommand();
            }
            else if (cmd == '\0')
            {
                throw new FormatException($"Path data must start with a command: '{d}'");
            }
            else if (cmd == 'M') cmd = 'L';   // implicit repeat after a moveto is a lineto
            else if (cmd == 'm') cmd = 'l';

            var rel = char.IsLower(cmd);
            var ox = rel ? cx : 0;
            var oy = rel ? cy : 0;

            switch (char.ToUpperInvariant(cmd))
            {
                case 'M':
                    cx = ox + reader.ReadNumber(); cy = oy + reader.ReadNumber();
                    sx = cx; sy = cy;
                    Emit(sb, 'M', cx, cy);
                    break;

                case 'L':
                    cx = ox + reader.ReadNumber(); cy = oy + reader.ReadNumber();
                    Emit(sb, 'L', cx, cy);
                    break;

                case 'H':
                    cx = ox + reader.ReadNumber();
                    Emit(sb, 'L', cx, cy);
                    break;

                case 'V':
                    cy = (rel ? cy : 0) + reader.ReadNumber();
                    Emit(sb, 'L', cx, cy);
                    break;

                case 'C':
                {
                    var x1 = ox + reader.ReadNumber(); var y1 = oy + reader.ReadNumber();
                    var x2 = ox + reader.ReadNumber(); var y2 = oy + reader.ReadNumber();
                    cx = ox + reader.ReadNumber(); cy = oy + reader.ReadNumber();
                    Emit(sb, 'C', x1, y1, x2, y2, cx, cy);
                    lcx = x2; lcy = y2;
                    break;
                }

                case 'S':
                {
                    // Reflect the previous cubic's second control point, or use the current point.
                    var prevCubic = "CcSs".IndexOf(lastCmd) >= 0;
                    var x1 = prevCubic ? 2 * cx - lcx : cx;
                    var y1 = prevCubic ? 2 * cy - lcy : cy;
                    var x2 = ox + reader.ReadNumber(); var y2 = oy + reader.ReadNumber();
                    cx = ox + reader.ReadNumber(); cy = oy + reader.ReadNumber();
                    Emit(sb, 'C', x1, y1, x2, y2, cx, cy);
                    lcx = x2; lcy = y2;
                    break;
                }

                case 'Q':
                {
                    var x1 = ox + reader.ReadNumber(); var y1 = oy + reader.ReadNumber();
                    cx = ox + reader.ReadNumber(); cy = oy + reader.ReadNumber();
                    Emit(sb, 'Q', x1, y1, cx, cy);
                    lcx = x1; lcy = y1;
                    break;
                }

                case 'T':
                {
                    var prevQuad = "QqTt".IndexOf(lastCmd) >= 0;
                    var x1 = prevQuad ? 2 * cx - lcx : cx;
                    var y1 = prevQuad ? 2 * cy - lcy : cy;
                    cx = ox + reader.ReadNumber(); cy = oy + reader.ReadNumber();
                    Emit(sb, 'Q', x1, y1, cx, cy);
                    lcx = x1; lcy = y1;
                    break;
                }

                case 'A':
                {
                    var rx = Math.Abs(reader.ReadNumber());
                    var ry = Math.Abs(reader.ReadNumber());
                    var rotation = reader.ReadNumber();
                    var large = reader.ReadFlag();
                    var sweep = reader.ReadFlag();
                    cx = ox + reader.ReadNumber(); cy = oy + reader.ReadNumber();
                    Arc(sb, rx, ry, rotation, large, sweep, cx, cy);
                    break;
                }

                case 'Z':
                    sb.Append(sb.Length == 0 ? "Z" : " Z");
                    cx = sx; cy = sy;
                    break;

                default:
                    throw new FormatException($"Unsupported path command '{cmd}' in '{d}'");
            }

            lastCmd = cmd;
        }

        return sb.ToString();
    }

    public static string Circle(double cx, double cy, double r) => Ellipse(cx, cy, r, r);

    public static string Ellipse(double cx, double cy, double rx, double ry)
    {
        var sb = new StringBuilder(64);
        Emit(sb, 'M', cx + rx, cy);
        Arc(sb, rx, ry, 0, true, true, cx - rx, cy);
        Arc(sb, rx, ry, 0, true, true, cx + rx, cy);
        sb.Append(" Z");
        return sb.ToString();
    }

    // SVG radius rules: a missing radius copies the other; each is clamped to half the side.
    public static string Rect(double x, double y, double w, double h, double? rxIn, double? ryIn)
    {
        var rx = rxIn ?? ryIn ?? 0;
        var ry = ryIn ?? rxIn ?? 0;
        rx = Math.Min(Math.Abs(rx), w / 2);
        ry = Math.Min(Math.Abs(ry), h / 2);

        var sb = new StringBuilder(96);
        if (rx <= 0 || ry <= 0)
        {
            Emit(sb, 'M', x, y);
            Emit(sb, 'L', x + w, y);
            Emit(sb, 'L', x + w, y + h);
            Emit(sb, 'L', x, y + h);
            sb.Append(" Z");
            return sb.ToString();
        }

        Emit(sb, 'M', x + rx, y);
        Emit(sb, 'L', x + w - rx, y);
        Arc(sb, rx, ry, 0, false, true, x + w, y + ry);
        Emit(sb, 'L', x + w, y + h - ry);
        Arc(sb, rx, ry, 0, false, true, x + w - rx, y + h);
        Emit(sb, 'L', x + rx, y + h);
        Arc(sb, rx, ry, 0, false, true, x, y + h - ry);
        Emit(sb, 'L', x, y + ry);
        Arc(sb, rx, ry, 0, false, true, x + rx, y);
        sb.Append(" Z");
        return sb.ToString();
    }

    public static string Line(double x1, double y1, double x2, double y2)
    {
        var sb = new StringBuilder(32);
        Emit(sb, 'M', x1, y1);
        Emit(sb, 'L', x2, y2);
        return sb.ToString();
    }

    public static string Poly(string points, bool close)
    {
        var reader = new Reader(points);
        var sb = new StringBuilder(points.Length + 16);
        var first = true;
        while (true)
        {
            reader.SkipSeparators();
            if (reader.AtEnd) break;
            var px = reader.ReadNumber();
            var py = reader.ReadNumber();
            Emit(sb, first ? 'M' : 'L', px, py);
            first = false;
        }
        if (close && !first) sb.Append(" Z");
        return sb.ToString();
    }

    // 3 decimals, invariant culture, never "-0".
    public static string Num(double v)
    {
        var rounded = Math.Round(v, 3, MidpointRounding.AwayFromZero);
        if (rounded == 0) return "0";   // also folds -0
        return rounded.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static void Emit(StringBuilder sb, char command, params double[] coords)
    {
        if (sb.Length > 0) sb.Append(' ');
        sb.Append(command);
        foreach (var c in coords) sb.Append(' ').Append(Num(c));
    }

    private static void Arc(StringBuilder sb, double rx, double ry, double rotation, bool large, bool sweep, double x, double y)
    {
        if (sb.Length > 0) sb.Append(' ');
        sb.Append("A ").Append(Num(rx)).Append(' ').Append(Num(ry)).Append(' ').Append(Num(rotation))
          .Append(large ? " 1" : " 0").Append(sweep ? " 1" : " 0")
          .Append(' ').Append(Num(x)).Append(' ').Append(Num(y));
    }

    /* Handles what a regex split gets wrong: run-together numbers (.53.53, 1-2), exponents,
       and arc flags without separators (a2 2 0 011 1 → flags 0, 1 then 1 1). */
    private sealed class Reader
    {
        private readonly string _s;
        private int _i;

        public Reader(string s) { _s = s ?? string.Empty; }

        public bool AtEnd => _i >= _s.Length;

        public void SkipSeparators()
        {
            while (_i < _s.Length && (char.IsWhiteSpace(_s[_i]) || _s[_i] == ',')) _i++;
        }

        public bool PeekIsCommand()
        {
            if (AtEnd) return false;
            var c = _s[_i];
            // 'e'/'E' only ever appear inside numbers, which ReadNumber consumes whole.
            return char.IsLetter(c) && c != 'e' && c != 'E';
        }

        public char ReadCommand() => _s[_i++];

        public bool ReadFlag()
        {
            SkipSeparators();
            if (AtEnd) throw new FormatException($"Expected arc flag at end of '{_s}'");
            var c = _s[_i++];
            if (c == '0') return false;
            if (c == '1') return true;
            throw new FormatException($"Invalid arc flag '{c}' at {_i - 1} in '{_s}'");
        }

        public double ReadNumber()
        {
            SkipSeparators();
            var start = _i;

            if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-')) _i++;

            var sawDigits = false;
            while (_i < _s.Length && char.IsDigit(_s[_i])) { _i++; sawDigits = true; }

            if (_i < _s.Length && _s[_i] == '.')
            {
                _i++;
                while (_i < _s.Length && char.IsDigit(_s[_i])) { _i++; sawDigits = true; }
            }

            if (!sawDigits) throw new FormatException($"Expected number at {start} in '{_s}'");

            if (_i < _s.Length && (_s[_i] == 'e' || _s[_i] == 'E'))
            {
                var save = _i;
                _i++;
                if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-')) _i++;
                var expDigits = false;
                while (_i < _s.Length && char.IsDigit(_s[_i])) { _i++; expDigits = true; }
                if (!expDigits) _i = save;   // a bare 'e' isn't an exponent
            }

            return double.Parse(_s.Substring(start, _i - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        }
    }
}
