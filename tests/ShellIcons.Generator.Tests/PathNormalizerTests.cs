using ShellIcons.Generator.Xaml;

namespace ShellIcons.Generator.Tests;

public class PathNormalizerTests
{
    private static string N(string d) => PathNormalizer.NormalizePathData(d);

    [Fact]
    public void CompactDecimals_RunTogether()
    {
        // ".53.53" is two numbers: 0.53 and 0.53
        Assert.Equal("M 0.53 0.53", N("M.53.53"));
    }

    [Fact]
    public void NegativeSign_StartsNewNumber()
    {
        Assert.Equal("M 1 -2 L 3 -0.44", N("M1-2L3-.44"));
    }

    [Fact]
    public void Exponent_IsPartOfTheNumber()
    {
        Assert.Equal("M 0 2 L 1 0", N("M1e-5 2L1 0"));
    }

    [Fact]
    public void CommasAndWhitespace_AreSeparators()
    {
        Assert.Equal("M 1 2 L 3 4", N("M 1,2\n\tL3 , 4"));
    }

    [Fact]
    public void CompactArcFlags_AreSingleCharacters()
    {
        // "a2 2 0 011 1" → rx=2 ry=2 rot=0 large=0 sweep=1, then x=1 y=1 (relative)
        Assert.Equal("M 10 10 A 2 2 0 0 1 11 11", N("M10 10a2 2 0 011 1"));
    }

    [Fact]
    public void ArcFlags_WithSeparators()
    {
        Assert.Equal("M 0 0 A 3 4 30 1 0 5 6", N("M0 0A3 4 30 1 0 5 6"));
    }

    [Fact]
    public void Relative_Commands_BecomeAbsolute()
    {
        Assert.Equal("M 5 5 L 8 9 L 8 12 L 2 12", N("m5 5l3 4v3h-6"));
    }

    [Fact]
    public void ImplicitRepeat_AfterMove_IsLine()
    {
        // extra pairs after m are relative linetos; after M, absolute linetos
        Assert.Equal("M 1 1 L 3 3 L 6 6", N("m1 1 2 2 3 3"));
        Assert.Equal("M 1 1 L 2 2 L 3 3", N("M1 1 2 2 3 3"));
    }

    [Fact]
    public void ImplicitRepeat_OfOtherCommands()
    {
        Assert.Equal("M 0 0 L 1 0 L 3 0", N("M0 0h1 2"));
    }

    [Fact]
    public void ClosePath_ResetsCurrentPointToSubpathStart()
    {
        // After z the current point is back at (10,10); l1 1 → (11,11)
        Assert.Equal("M 10 10 L 20 10 Z L 11 11", N("M10 10h10zl1 1"));
    }

    [Fact]
    public void RelativeCubic_AllPointsRelativeToSegmentStart()
    {
        Assert.Equal("M 10 10 C 11 11 12 12 13 13", N("M10 10c1 1 2 2 3 3"));
    }

    [Fact]
    public void SmoothCubic_ReflectsPreviousControlPoint()
    {
        // prev second control (12,10) reflected about (14,10) → (16,10)
        Assert.Equal("M 10 10 C 10 12 12 10 14 10 C 16 10 18 12 20 10", N("M10 10C10 12 12 10 14 10S18 12 20 10"));
    }

    [Fact]
    public void SmoothCubic_WithoutPreviousCubic_UsesCurrentPoint()
    {
        Assert.Equal("M 10 10 C 10 10 12 12 14 10", N("M10 10S12 12 14 10"));
    }

    [Fact]
    public void RelativeSmoothCubic()
    {
        Assert.Equal("M 0 0 C 0 2 2 2 2 0 C 2 -2 4 -2 4 0", N("M0 0c0 2 2 2 2 0s2-2 2 0"));
    }

    [Fact]
    public void Quadratic_AndSmoothQuadraticReflection()
    {
        Assert.Equal("M 0 0 Q 1 2 2 0 Q 3 -2 4 0", N("M0 0Q1 2 2 0T4 0"));
    }

    [Fact]
    public void Rounding_ThreeDecimals_NoNegativeZero()
    {
        Assert.Equal("M 1.235 0 L 0 0", N("M1.23456-0.0001L0 0"));
        Assert.Equal("0", PathNormalizer.Num(-0.0004));
        Assert.Equal("-0.5", PathNormalizer.Num(-0.5));
    }

    [Fact]
    public void RealLucidePath_House()
    {
        Assert.Equal(
            "M 15 21 L 15 13 A 1 1 0 0 0 14 12 L 10 12 A 1 1 0 0 0 9 13 L 9 21",
            N("M15 21v-8a1 1 0 0 0-1-1h-4a1 1 0 0 0-1 1v8"));
    }

    [Fact]
    public void InvalidArcFlag_Throws()
    {
        Assert.Throws<FormatException>(() => N("M0 0a2 2 0 2 1 1 1"));
    }

    [Fact]
    public void Circle_IsTwoHalfArcs()
    {
        Assert.Equal("M 22 12 A 10 10 0 1 1 2 12 A 10 10 0 1 1 22 12 Z", PathNormalizer.Circle(12, 12, 10));
    }

    [Fact]
    public void Ellipse_UsesBothRadii()
    {
        Assert.Equal("M 15 12 A 3 5 0 1 1 9 12 A 3 5 0 1 1 15 12 Z", PathNormalizer.Ellipse(12, 12, 3, 5));
    }

    [Fact]
    public void Rect_WithoutRadius_IsFourLines()
    {
        Assert.Equal("M 2 3 L 12 3 L 12 8 L 2 8 Z", PathNormalizer.Rect(2, 3, 10, 5, null, null));
    }

    [Fact]
    public void Rect_WithRadius_HasFourArcCorners()
    {
        Assert.Equal(
            "M 4 2 L 20 2 A 2 2 0 0 1 22 4 L 22 20 A 2 2 0 0 1 20 22 L 4 22 A 2 2 0 0 1 2 20 L 2 4 A 2 2 0 0 1 4 2 Z",
            PathNormalizer.Rect(2, 2, 20, 20, 2, null));
    }

    [Fact]
    public void Rect_MissingRy_CopiesRx_AndRadiusIsClampedToHalfSide()
    {
        // rx=10 on a 4-tall rect: ry copies rx (10) then both clamp — rx to w/2=5, ry to h/2=2
        var d = PathNormalizer.Rect(0, 0, 10, 4, 10, null);
        Assert.StartsWith("M 5 0 L 5 0 A 5 2 0 0 1 10 2", d);
    }

    [Fact]
    public void Line_IsMoveThenLine()
    {
        Assert.Equal("M 12 17 L 12.01 17", PathNormalizer.Line(12, 17, 12.01, 17));
    }

    [Fact]
    public void Polyline_IsOpen_PolygonIsClosed()
    {
        Assert.Equal("M 1 2 L 3 4 L 5 6", PathNormalizer.Poly("1,2 3,4 5,6", close: false));
        Assert.Equal("M 1 2 L 3 4 L 5 6 Z", PathNormalizer.Poly("1 2 3 4 5 6", close: true));
    }
}

public class SvgShapeReaderTests
{
    [Fact]
    public void StrokeShapes_MergeIntoOnePath()
    {
        var svg = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <circle cx="11" cy="11" r="8" />
              <path d="m21 21-4.3-4.3" />
            </svg>
            """;

        var shapes = SvgShapeReader.Read(svg);

        Assert.Equal("M 19 11 A 8 8 0 1 1 3 11 A 8 8 0 1 1 19 11 Z M 21 21 L 16.7 16.7", shapes.StrokeData);
        Assert.Null(shapes.FillData);
        Assert.Empty(shapes.Unsupported);
    }

    [Fact]
    public void FillCurrentColor_GoesToTheFillPath()
    {
        var svg = """
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M3 3h18" />
              <circle cx="12" cy="12" r="1" fill="currentColor" />
            </svg>
            """;

        var shapes = SvgShapeReader.Read(svg);

        Assert.Equal("M 3 3 L 21 3", shapes.StrokeData);
        Assert.Equal("M 13 12 A 1 1 0 1 1 11 12 A 1 1 0 1 1 13 12 Z", shapes.FillData);
    }

    [Fact]
    public void GroupsAndTransforms_AreReportedAsUnsupported()
    {
        var svg = """
            <svg viewBox="0 0 24 24">
              <g><path d="M1 1h2" transform="rotate(45)" /></g>
            </svg>
            """;

        var shapes = SvgShapeReader.Read(svg);

        Assert.Contains("<g>", shapes.Unsupported);
        Assert.Contains("<path transform>", shapes.Unsupported);
    }

    [Fact]
    public void RectAttributes_Parsed()
    {
        var shapes = SvgShapeReader.Read("""<svg><rect width="18" height="18" x="3" y="3" rx="2" /></svg>""");
        Assert.StartsWith("M 5 3 L 19 3 A 2 2 0 0 1 21 5", shapes.StrokeData);
    }

    [Fact]
    public void Polygon_AndPolyline_Parsed()
    {
        var shapes = SvgShapeReader.Read("""<svg><polygon points="12 2 15 8 9 8" /><polyline points="1 1 2 2" /></svg>""");
        Assert.Equal("M 12 2 L 15 8 L 9 8 Z M 1 1 L 2 2", shapes.StrokeData);
    }
}

public class OffCanvasTests
{
    [Fact]
    public void ShapeEntirelyOutsideViewBox_IsDropped()
    {
        var shapes = SvgShapeReader.Read("""<svg><path d="m2 2 20 20" /><path d="M29.5 11.5s5 5 4 5" /></svg>""");

        Assert.Equal("M 2 2 L 22 22", shapes.StrokeData);
        Assert.Single(shapes.OffCanvas);
    }

    [Theory]
    [InlineData("M 12 24.8 L 13 24.8")]        // off-grid, but its stroke reaches into the bottom edge
    [InlineData("M 30 12 A 10 10 0 0 1 30 14")] // arc endpoints outside, but the radius reaches in
    [InlineData("M 22 4 C 22 4 25 10 20 24")]   // control point outside, curve inside
    public void ShapeThatCanReachTheViewBox_IsKept(string data)
    {
        Assert.False(SvgShapeReader.IsOffCanvas(data));
    }
}

public class IconMetadataReaderTests
{
    [Fact]
    public void ReadsTagsCategoriesAliases()
    {
        var json = """
            {
              "$schema": "../icon.schema.json",
              "contributors": ["jguddas"],
              "tags": ["home", "living", "building"],
              "categories": ["buildings", "home"],
              "aliases": ["home"]
            }
            """;

        var meta = IconMetadataReader.Read(json);

        Assert.Equal(new[] { "home", "living", "building" }, meta.Tags);
        Assert.Equal(new[] { "buildings", "home" }, meta.Categories);
        Assert.Equal(new[] { "home" }, meta.Aliases);
    }

    [Fact]
    public void ObjectAliases_TakeOnlyTheName()
    {
        var json = """{ "aliases": [ { "name": "home", "deprecated": true, "deprecationReason": "renamed" } ] }""";
        Assert.Equal(new[] { "home" }, IconMetadataReader.Read(json).Aliases);
    }

    [Fact]
    public void MissingKeys_AreEmpty()
    {
        var meta = IconMetadataReader.Read("""{ "tags": [] }""");
        Assert.Empty(meta.Tags);
        Assert.Empty(meta.Categories);
        Assert.Empty(meta.Aliases);
    }
}
