using Bunit;
using AngleSharp.Dom;
using ShellIcons.Icons;

namespace ShellIcons.Blazor.Tests;

/// <summary>
/// Integration tests that render icons the source generator emitted, to prove
/// the emitted code is functional end-to-end (not just IconCore in isolation).
/// </summary>
public class GeneratedIconTests : IDisposable
{
    private readonly TestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public void ChevronRight_HasExpectedLucidePath()
    {
        var path = _ctx.RenderComponent<ChevronRight>().Find("svg > path");
        Assert.Equal("m9 18 6-6-6-6", path.GetAttribute("d"));
    }

    [Fact]
    public void CircleCheck_EmitsCircleAndPath()
    {
        var cut = _ctx.RenderComponent<CircleCheck>();
        var circle = cut.Find("svg > circle");
        var path = cut.Find("svg > path");

        Assert.Equal("12", circle.GetAttribute("cx"));
        Assert.Equal("12", circle.GetAttribute("cy"));
        Assert.Equal("10", circle.GetAttribute("r"));
        Assert.Equal("m9 12 2 2 4-4", path.GetAttribute("d"));
    }

    [Fact]
    public void House_HasTwoPaths()
    {
        var paths = _ctx.RenderComponent<House>().FindAll("svg > path");
        Assert.Equal(2, paths.Count);
    }

    [Fact]
    public void Zap_UsesCurrentColorStroke()
    {
        // The Lucide contract lives on IconCore, so every generated icon inherits it.
        // This test guards against a generator regression that would override it.
        var svg = _ctx.RenderComponent<Zap>().Find("svg");
        Assert.Equal("currentColor", svg.GetAttribute("stroke"));
        Assert.Equal("none", svg.GetAttribute("fill"));
    }
}
