using Microsoft.Maui.Controls.Shapes;

namespace ShellIcons.Maui.Tests;

// A relative→absolute bug pushes points off the 24×24 grid, so checking every icon catches it.
public class GeometryTests
{
    [Fact]
    public void EveryIcon_Parses_AndStaysOnTheGrid()
    {
        var failures = new List<string>();

        foreach (var name in Enum.GetValues<IconName>().Where(n => n != IconName.None))
        {
            var (stroke, fill) = IconData.Get(name);
            if (string.IsNullOrEmpty(stroke) && fill is null)
            {
                failures.Add($"{name}: no shapes");
                continue;
            }

            foreach (var data in new[] { stroke, fill }.Where(d => !string.IsNullOrEmpty(d)))
            {
                PathGeometry geometry;
                try { geometry = IconGeometry.Parse(data!); }
                catch (Exception ex) { failures.Add($"{name}: {ex.Message}"); continue; }

                if (geometry.Figures.Count == 0) failures.Add($"{name}: no figures");

                // Control points may poke out while the curve stays inside (twitter's does, by 0.7).
                foreach (var (p, isControl) in Points(geometry))
                {
                    var tolerance = isControl ? 6 : 0.5;
                    if (p.X < -tolerance || p.X > 24 + tolerance || p.Y < -tolerance || p.Y > 24 + tolerance)
                    {
                        failures.Add($"{name}: {(isControl ? "control point" : "point")} ({p.X}, {p.Y}) is off the 24x24 grid");
                        break;
                    }
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures.Take(25)));
    }

    [Fact]
    public void FillShapes_AreSplitIntoTheirOwnPath()
    {
        var withFill = Enum.GetValues<IconName>().Where(n => IconData.Get(n).Fill is not null).ToList();

        Assert.NotEmpty(withFill);
        Assert.True(withFill.Count < 50);
    }

    [Fact]
    public void KnownIcon_ChevronRight_ExactPath()
    {
        Assert.Equal(("M 9 18 L 15 12 L 9 6", (string?)null), IconData.Get(IconName.ChevronRight));
    }

    [Fact]
    public void CircleCheck_CircleBecomesArcs()
    {
        var (stroke, _) = IconData.Get(IconName.CircleCheck);
        Assert.Equal("M 22 12 A 10 10 0 1 1 2 12 A 10 10 0 1 1 22 12 Z M 9 12 L 11 14 L 15 10", stroke);
    }

    [Fact]
    public void Parse_CommandAfterClose_StartsNewFigureAtSubpathStart()
    {
        var g = IconGeometry.Parse("M 10 10 L 20 10 Z L 11 11");

        Assert.Equal(2, g.Figures.Count);
        Assert.True(g.Figures[0].IsClosed);
        Assert.Equal(new Point(10, 10), g.Figures[1].StartPoint);
    }

    [Fact]
    public void Parse_Arc_MapsSvgSweepFlagToClockwise()
    {
        var arc = (ArcSegment)IconGeometry.Parse("M 0 0 A 2 3 45 1 1 4 4").Figures[0].Segments[0];

        Assert.Equal(new Size(2, 3), arc.Size);
        Assert.Equal(45, arc.RotationAngle);
        Assert.True(arc.IsLargeArc);
        Assert.Equal(SweepDirection.Clockwise, arc.SweepDirection);
        Assert.Equal(new Point(4, 4), arc.Point);
    }

    [Fact]
    public void Geometry_IsCachedPerIcon()
    {
        Assert.Same(IconGeometry.Get("M 1 1 L 2 2"), IconGeometry.Get("M 1 1 L 2 2"));
    }

    [Fact]
    public void SaveOff_StrayOffCanvasShapeFromLucide_IsDropped()
    {
        // Lucide's save-off.svg has a stray `M29.5 11.5s5 5 4 5` outside the viewBox.
        var (stroke, _) = IconData.Get(IconName.SaveOff);
        Assert.DoesNotContain("29.5", stroke);
        Assert.Contains("M 2 2 L 22 22", stroke);   // the real slash is still there
    }

    private static IEnumerable<(Point Point, bool IsControl)> Points(PathGeometry geometry)
    {
        foreach (var figure in geometry.Figures)
        {
            yield return (figure.StartPoint, false);
            foreach (var segment in figure.Segments)
            {
                switch (segment)
                {
                    case LineSegment l: yield return (l.Point, false); break;
                    case BezierSegment b: yield return (b.Point1, true); yield return (b.Point2, true); yield return (b.Point3, false); break;
                    case QuadraticBezierSegment q: yield return (q.Point1, true); yield return (q.Point2, false); break;
                    case ArcSegment a: yield return (a.Point, false); break;
                }
            }
        }
    }
}
