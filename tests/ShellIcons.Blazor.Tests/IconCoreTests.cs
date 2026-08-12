using Bunit;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;

namespace ShellIcons.Blazor.Tests;

public class IconCoreTests : IDisposable
{
    private readonly TestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    // ---- Default render ---------------------------------------------------

    [Fact]
    public void Default_EmitsLucideContractAttributes()
    {
        var svg = _ctx.RenderComponent<TestIcon>().Find("svg");

        Assert.Equal("http://www.w3.org/2000/svg", svg.GetAttribute("xmlns"));
        Assert.Equal("24", svg.GetAttribute("width"));
        Assert.Equal("24", svg.GetAttribute("height"));
        Assert.Equal("0 0 24 24", svg.GetAttribute("viewBox"));
        Assert.Equal("none", svg.GetAttribute("fill"));
        Assert.Equal("currentColor", svg.GetAttribute("stroke"));
        Assert.Equal("2", svg.GetAttribute("stroke-width"));
        Assert.Equal("round", svg.GetAttribute("stroke-linecap"));
        Assert.Equal("round", svg.GetAttribute("stroke-linejoin"));
    }

    [Fact]
    public void Default_EmitsInnerShapeMarkup()
    {
        var path = _ctx.RenderComponent<TestIcon>().Find("svg > path");
        Assert.Equal(TestIcon.PathData, path.GetAttribute("d"));
    }

    // ---- Size prop --------------------------------------------------------

    [Theory]
    [InlineData("16")]
    [InlineData("48")]
    [InlineData("1.5em")]
    [InlineData("100%")]
    public void Size_ForwardedToWidthAndHeight(string size)
    {
        var svg = _ctx.RenderComponent<TestIcon>(p => p.Add(x => x.Size, size)).Find("svg");
        Assert.Equal(size, svg.GetAttribute("width"));
        Assert.Equal(size, svg.GetAttribute("height"));
    }

    // ---- StrokeWidth prop -------------------------------------------------

    [Theory]
    [InlineData(1.0, "1")]
    [InlineData(1.5, "1.5")]
    [InlineData(0.75, "0.75")]
    [InlineData(3.0, "3")]
    public void StrokeWidth_ForwardedInInvariantCulture(double value, string expected)
    {
        var svg = _ctx.RenderComponent<TestIcon>(p => p.Add(x => x.StrokeWidth, value)).Find("svg");
        Assert.Equal(expected, svg.GetAttribute("stroke-width"));
    }

    // ---- AbsoluteStroke math ---------------------------------------------

    [Fact]
    public void AbsoluteStroke_ScalesStrokeWidthByInverseSize()
    {
        // At Size=16, StrokeWidth=2, absolute -> emitted stroke-width = 2 * 24 / 16 = 3
        var svg = _ctx.RenderComponent<TestIcon>(p => p
                .Add(x => x.Size, "16")
                .Add(x => x.StrokeWidth, 2.0)
                .Add(x => x.AbsoluteStroke, true))
            .Find("svg");

        Assert.Equal("3", svg.GetAttribute("stroke-width"));
    }

    [Fact]
    public void AbsoluteStroke_At48_HalvesStrokeWidth()
    {
        // At Size=48, StrokeWidth=2, absolute -> 2 * 24 / 48 = 1
        var svg = _ctx.RenderComponent<TestIcon>(p => p
                .Add(x => x.Size, "48")
                .Add(x => x.StrokeWidth, 2.0)
                .Add(x => x.AbsoluteStroke, true))
            .Find("svg");

        Assert.Equal("1", svg.GetAttribute("stroke-width"));
    }

    [Fact]
    public void AbsoluteStroke_WithNonNumericSize_FallsBackToRawStrokeWidth()
    {
        // "1.5em" can't participate in the scaling math — fall back to StrokeWidth as-is.
        var svg = _ctx.RenderComponent<TestIcon>(p => p
                .Add(x => x.Size, "1.5em")
                .Add(x => x.StrokeWidth, 2.5)
                .Add(x => x.AbsoluteStroke, true))
            .Find("svg");

        Assert.Equal("2.5", svg.GetAttribute("stroke-width"));
    }

    // ---- Class prop -------------------------------------------------------

    [Fact]
    public void Class_ForwardedToSvg()
    {
        var svg = _ctx.RenderComponent<TestIcon>(p => p.Add(x => x.Class, "text-primary lg")).Find("svg");
        Assert.Equal("text-primary lg", svg.GetAttribute("class"));
    }

    [Fact]
    public void Class_Null_NotEmittedOnSvg()
    {
        var svg = _ctx.RenderComponent<TestIcon>().Find("svg");
        Assert.False(svg.HasAttribute("class"));
    }

    // ---- Accessibility ----------------------------------------------------

    [Fact]
    public void NoTitle_MarksSvgAriaHidden()
    {
        var svg = _ctx.RenderComponent<TestIcon>().Find("svg");
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.False(svg.HasAttribute("role"));
        Assert.False(svg.HasAttribute("aria-labelledby"));
        Assert.Empty(svg.QuerySelectorAll("title"));
    }

    [Fact]
    public void Title_EmitsRoleImgAndLabelledByTitle()
    {
        var cut = _ctx.RenderComponent<TestIcon>(p => p.Add(x => x.Title, "Go to next page"));
        var svg = cut.Find("svg");
        var title = cut.Find("svg > title");

        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.False(svg.HasAttribute("aria-hidden"));

        var labelledBy = svg.GetAttribute("aria-labelledby");
        Assert.False(string.IsNullOrWhiteSpace(labelledBy));
        Assert.StartsWith("shellicon-", labelledBy);
        Assert.Equal(labelledBy, title.GetAttribute("id"));

        Assert.Equal("Go to next page", title.TextContent);
    }

    [Fact]
    public void Title_MultipleIcons_HaveDistinctIds()
    {
        var a = _ctx.RenderComponent<TestIcon>(p => p.Add(x => x.Title, "A")).Find("svg").GetAttribute("aria-labelledby");
        var b = _ctx.RenderComponent<TestIcon>(p => p.Add(x => x.Title, "B")).Find("svg").GetAttribute("aria-labelledby");

        Assert.NotEqual(a, b);
    }

    // ---- Additional attribute splat --------------------------------------

    [Fact]
    public void AdditionalAttributes_ForwardedToSvg()
    {
        var svg = _ctx.RenderComponent<TestIcon>(p => p
                .AddUnmatched("data-testid", "my-icon")
                .AddUnmatched("style", "opacity: 0.5"))
            .Find("svg");

        Assert.Equal("my-icon", svg.GetAttribute("data-testid"));
        Assert.Equal("opacity: 0.5", svg.GetAttribute("style"));
    }
}
