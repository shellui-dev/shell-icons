using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace ShellIcons.Maui;

/* Walks the generator's pre-normalized path data (no SVG parsing, no PathGeometryConverter).
   Geometry is built at the icon's size and cached, so every control showing the same icon at the
   same size shares one geometry. */
internal static class IconGeometry
{
    private static readonly ConcurrentDictionary<(string Data, double Scale), Geometry> Cache = new();

    public static Geometry? Get(string? data, double scale = 1) =>
        string.IsNullOrEmpty(data) ? null : Cache.GetOrAdd((data, scale), static key => Parse(key.Data, key.Scale));

    internal static PathGeometry Parse(string data, double scale = 1)
    {
        var geometry = new PathGeometry();
        var tokens = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        PathFigure? figure = null;
        var subpathStart = Point.Zero;
        var i = 0;

        while (i < tokens.Length)
        {
            var command = tokens[i++];
            switch (command)
            {
                case "M":
                    subpathStart = ReadPoint(tokens, ref i, scale);
                    figure = NewFigure(geometry, subpathStart);
                    break;

                case "L":
                    Current(geometry, ref figure, subpathStart).Segments.Add(new LineSegment(ReadPoint(tokens, ref i, scale)));
                    break;

                case "C":
                {
                    var target = Current(geometry, ref figure, subpathStart);
                    target.Segments.Add(new BezierSegment(ReadPoint(tokens, ref i, scale), ReadPoint(tokens, ref i, scale), ReadPoint(tokens, ref i, scale)));
                    break;
                }

                case "Q":
                {
                    var target = Current(geometry, ref figure, subpathStart);
                    target.Segments.Add(new QuadraticBezierSegment(ReadPoint(tokens, ref i, scale), ReadPoint(tokens, ref i, scale)));
                    break;
                }

                case "A":
                {
                    var target = Current(geometry, ref figure, subpathStart);
                    var size = new Size(ReadDouble(tokens, ref i) * scale, ReadDouble(tokens, ref i) * scale);
                    var rotation = ReadDouble(tokens, ref i);
                    var isLargeArc = tokens[i++] == "1";
                    // SVG sweep-flag 1 is clockwise in y-down coordinates, as in MAUI.
                    var sweep = tokens[i++] == "1" ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;
                    target.Segments.Add(new ArcSegment(ReadPoint(tokens, ref i, scale), size, rotation, sweep, isLargeArc));
                    break;
                }

                case "Z":
                    if (figure is not null) figure.IsClosed = true;
                    // A command after Z without an M starts a new sub-path at the closed one's start.
                    figure = null;
                    break;

                default:
                    throw new FormatException($"Unexpected token '{command}' in icon path data.");
            }
        }

        return geometry;
    }

    private static PathFigure Current(PathGeometry geometry, ref PathFigure? figure, Point subpathStart) =>
        figure ??= NewFigure(geometry, subpathStart);

    private static PathFigure NewFigure(PathGeometry geometry, Point start)
    {
        var figure = new PathFigure { StartPoint = start, IsClosed = false, IsFilled = true };
        geometry.Figures.Add(figure);
        return figure;
    }

    private static Point ReadPoint(string[] tokens, ref int i, double scale) =>
        new(ReadDouble(tokens, ref i) * scale, ReadDouble(tokens, ref i) * scale);

    private static double ReadDouble(string[] tokens, ref int i) =>
        double.Parse(tokens[i++], NumberStyles.Float, CultureInfo.InvariantCulture);
}
