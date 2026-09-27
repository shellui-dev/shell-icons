using Bunit;
using Microsoft.AspNetCore.Components;
using AngleSharp.Dom;

namespace ShellIcons.Blazor.Tests;

public class IconFactoryTests : IDisposable
{
    private readonly TestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public void Factory_RendersMatchingShape()
    {
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
        Assert.Null(svg.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void Factory_NoTitle_IsDecorative()
    {
        var svg = _ctx.Render(Icon.House()).Find("svg");

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

    [Fact]
    public void Factory_AllDefaults_ReturnsCachedFragment()
    {
        // No-arg calls must not allocate a new delegate per render.
        Assert.Same(Icon.Plus(), Icon.Plus());
        Assert.NotSame(Icon.Plus(), Icon.Minus());
    }

    [Fact]
    public void Factory_NonDefaultArgs_ReturnsFreshFragment()
    {
        Assert.NotSame(Icon.Plus(size: "16"), Icon.Plus(size: "16"));
    }

    [Fact]
    public void Factory_DefaultArgs_RenderLucideDefaults()
    {
        var svg = _ctx.Render(Icon.Plus()).Find("svg");

        Assert.Equal("24", svg.GetAttribute("width"));
        Assert.Equal("2", svg.GetAttribute("stroke-width"));
        Assert.Null(svg.GetAttribute("class"));
    }

    [Fact]
    public void Factory_OnClickViaAdditionalAttributes_UsesPlainEventName()
    {
        // "onclick", not "@onclick": the @ form is Razor markup syntax only.
        var clicks = 0;
        var extras = new Dictionary<string, object>
        {
            ["onclick"] = EventCallback.Factory.Create(this, () => clicks++)
        };

        var cut = _ctx.Render(Icon.Bell(additionalAttributes: extras));
        cut.Find("svg").Click();

        Assert.Equal(1, clicks);
    }
}
