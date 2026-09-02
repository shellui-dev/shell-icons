using Bunit;
using AngleSharp.Dom;

namespace ShellIcons.Blazor.Tests;

// Tests for the static `Icon` factory dispatcher (Icon.Plus(), Icon.Bell(), etc.)
// added for consumers who can't import the flat `ShellIcons.Icons` namespace
// without colliding with UI-library types (Badge, Table, Router, …).
public class IconFactoryTests : IDisposable
{
    private readonly TestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public void Factory_RendersMatchingShape()
    {
        // chevron-right's Lucide path data
        var cut = _ctx.Render(Icon.ChevronRight());
        var path = cut.Find("svg > path");

        Assert.Equal("m9 18 6-6-6-6", path.GetAttribute("d"));
    }

    [Fact]
    public void Factory_ForwardsSizeAndStrokeWidth()
    {
        var svg = _ctx.Render(Icon.Zap(size: "16", strokeWidth: 1.5)).Find("svg");

        Assert.Equal("16", svg.GetAttribute("width"));
        Assert.Equal("16", svg.GetAttribute("height"));
        Assert.Equal("1.5", svg.GetAttribute("stroke-width"));
    }

    [Fact]
    public void Factory_ForwardsCssClass()
    {
        var svg = _ctx.Render(Icon.TriangleAlert(cssClass: "text-warning")).Find("svg");

        Assert.Equal("text-warning", svg.GetAttribute("class"));
    }

    [Fact]
    public void Factory_TitleSwitchesToImgRole()
    {
        var svg = _ctx.Render(Icon.X(title: "Close dialog")).Find("svg");

        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.NotNull(svg.GetAttribute("aria-labelledby"));
        // aria-hidden should NOT be set when a title is provided
        Assert.Null(svg.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Factory_NoTitle_IsDecorative()
    {
        var svg = _ctx.Render(Icon.House()).Find("svg");

        // Decorative: hidden from AT, no role/title id
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Null(svg.GetAttribute("role"));
    }

    [Fact]
    public void Factory_ForwardsAdditionalAttributes()
    {
        var extras = new Dictionary<string, object>
        {
            ["data-testid"] = "my-icon",
            ["style"] = "color: crimson"
        };

        var svg = _ctx.Render(Icon.Bell(additionalAttributes: extras)).Find("svg");

        Assert.Equal("my-icon", svg.GetAttribute("data-testid"));
        Assert.Equal("color: crimson", svg.GetAttribute("style"));
    }
}
