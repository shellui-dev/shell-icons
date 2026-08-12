using Bunit;
using AngleSharp.Dom;

namespace ShellIcons.Blazor.Tests;

public class ShellIconDispatcherTests : IDisposable
{
    private readonly TestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public void KnownName_RendersMatchingShape()
    {
        // chevron-right's shape data — matches the Lucide 0.475.0 source.
        var path = _ctx.RenderComponent<ShellIcon>(p => p.Add(x => x.Name, "chevron-right"))
            .Find("svg > path");

        Assert.Equal("m9 18 6-6-6-6", path.GetAttribute("d"));
    }

    [Fact]
    public void UnknownName_RendersEmptySvg()
    {
        var svg = _ctx.RenderComponent<ShellIcon>(p => p.Add(x => x.Name, "not-a-real-icon-name"))
            .Find("svg");

        // The svg wrapper is still emitted, but no shape children.
        Assert.Empty(svg.QuerySelectorAll("path,circle,rect,line,polyline,polygon"));
    }

    [Fact]
    public void Names_ContainsFullCatalog()
    {
        // Lucide 0.475.0 ships 1500+ icons. Assert we're well over the minimum.
        Assert.True(ShellIcon.Names.Count > 1000,
            $"Expected >1000 icons in the catalog, got {ShellIcon.Names.Count}");

        // And that a handful of expected members are present.
        Assert.Contains("chevron-right", ShellIcon.Names);
        Assert.Contains("house", ShellIcon.Names);
        Assert.Contains("zap", ShellIcon.Names);
        Assert.Contains("triangle-alert", ShellIcon.Names);
        Assert.Contains("circle-check", ShellIcon.Names);
    }
}
