using Bunit;
using AngleSharp.Dom;
using CollisionFixture;

namespace ShellIcons.Blazor.Tests;

public class SuffixedIconTests : IDisposable
{
    private readonly TestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public void Suffixed_RendersSameShapeAsFlatComponent()
    {
        var path = _ctx.RenderComponent<ChevronRightIcon>().Find("svg > path");
        Assert.Equal("m9 18 6-6-6-6", path.GetAttribute("d"));
    }

    [Fact]
    public void Suffixed_LivesInRootNamespace_AndDerivesFromFlatComponent()
    {
        Assert.Equal("ShellIcons", typeof(ChevronRightIcon).Namespace);
        Assert.Equal(typeof(Icons.ChevronRight), typeof(ChevronRightIcon).BaseType);
    }

    [Fact]
    public void Suffixed_CoversCatalog_ExceptReservedShell()
    {
        var suffixed = typeof(IconCore).Assembly.GetTypes()
            .Where(t => t.Namespace == "ShellIcons" && t.Name.EndsWith("Icon") && t.IsSubclassOf(typeof(IconCore)))
            .Where(t => t != typeof(ShellIcon))
            .ToArray();

        // `shell` has no alias: ShellIcon is the dispatcher.
        Assert.Equal(ShellIcon.Names.Count - 1, suffixed.Length);
        Assert.Contains("shell", ShellIcon.Names);
    }

    [Fact]
    public void ShellIcon_IsStillTheDispatcher()
    {
        Assert.NotNull(typeof(ShellIcon).GetProperty(nameof(ShellIcon.Name)));
        var path = _ctx.RenderComponent<ShellIcon>(p => p.Add(x => x.Name, "chevron-right")).Find("svg > path");
        Assert.Equal("m9 18 6-6-6-6", path.GetAttribute("d"));
    }

    [Fact]
    public void ShellSeashell_ReachableThroughFactory()
    {
        var svg = _ctx.Render(Icon.Shell()).Find("svg");
        Assert.NotEmpty(svg.Children);
    }

    [Fact]
    public void RazorFixture_CompilesNextToCollidingUiKit_AndRendersEverything()
    {
        var cut = _ctx.RenderComponent<CollisionPage>();

        Assert.Equal("New", cut.Find("span.fake-badge").TextContent);           // UI kit's Badge
        Assert.NotEmpty(cut.Find("svg.markup-icon").Children);                  // <BadgeIcon />
        Assert.NotEmpty(cut.Find("svg.router-icon").Children);                  // <RouterIcon />
        Assert.NotEmpty(cut.Find("svg.factory-icon").Children);                 // @Icon.Table()
        Assert.Equal("16", cut.Find("svg.markup-icon").GetAttribute("width"));
    }

    [Fact]
    public void RazorFixture_OnClickWithoutAt_ReachesTheSvg()
    {
        var cut = _ctx.RenderComponent<CollisionPage>();

        cut.Find("svg.markup-icon").Click();
        cut.Find("svg.markup-icon").Click();

        Assert.Equal(2, cut.Instance.Clicks);
    }

    [Fact]
    public void EventDirectiveOnComponent_ThrowsWithGuidance()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => _ctx.RenderComponent<EventDirectiveMisuse>());

        Assert.Contains("@onclick", ex.Message);
        Assert.Contains("wrapping <button", ex.Message);
        Assert.Contains("onclick=\"@(() => Handler())\"", ex.Message);
    }
}
