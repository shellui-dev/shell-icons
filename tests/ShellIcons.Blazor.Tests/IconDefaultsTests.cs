using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace ShellIcons.Blazor.Tests;

public class IconDefaultsTests : IDisposable
{
    private readonly TestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private IRenderedComponent<IconDefaults> Defaults(Action<ComponentParameterCollectionBuilder<IconDefaults>> parameters) =>
        _ctx.RenderComponent<IconDefaults>(parameters);

    [Fact]
    public void IconsInside_TakeTheDefaults()
    {
        var svg = Defaults(p => p
                .Add(x => x.Size, "16")
                .Add(x => x.StrokeWidth, 1.5)
                .Add(x => x.Class, "shrink-0")
                .AddChildContent<ZapIcon>())
            .Find("svg");

        Assert.Equal("16", svg.GetAttribute("width"));
        Assert.Equal("16", svg.GetAttribute("height"));
        Assert.Equal("1.5", svg.GetAttribute("stroke-width"));
        Assert.Equal("shrink-0", svg.GetAttribute("class"));
    }

    [Fact]
    public void ValuesSetOnTheIcon_Win_EvenWhenTheyEqualTheBuiltInDefaults()
    {
        var svg = Defaults(p => p
                .Add(x => x.Size, "16")
                .Add(x => x.StrokeWidth, 1.5)
                .AddChildContent<ZapIcon>(i => i.Add(x => x.Size, "24").Add(x => x.StrokeWidth, 2)))
            .Find("svg");

        Assert.Equal("24", svg.GetAttribute("width"));
        Assert.Equal("2", svg.GetAttribute("stroke-width"));
    }

    [Fact]
    public void Classes_AreCombined_DefaultsFirst()
    {
        var svg = Defaults(p => p
                .Add(x => x.Class, "shrink-0")
                .AddChildContent<ZapIcon>(i => i.Add(x => x.Class, "text-warning")))
            .Find("svg");

        Assert.Equal("shrink-0 text-warning", svg.GetAttribute("class"));
    }

    [Fact]
    public void NestedDefaults_OverrideOuterOnes_AndInheritTheRest()
    {
        RenderFragment inner = b =>
        {
            b.OpenComponent<IconDefaults>(0);
            b.AddAttribute(1, nameof(IconDefaults.Size), "20");
            b.AddAttribute(2, nameof(IconDefaults.Class), "inner");
            b.AddAttribute(3, nameof(IconDefaults.ChildContent), (RenderFragment)(c =>
            {
                c.OpenComponent<ZapIcon>(0);
                c.CloseComponent();
            }));
            b.CloseComponent();
        };

        var svg = Defaults(p => p
                .Add(x => x.Size, "16")
                .Add(x => x.StrokeWidth, 1.5)
                .Add(x => x.Class, "outer")
                .AddChildContent(inner))
            .Find("svg");

        Assert.Equal("20", svg.GetAttribute("width"));
        Assert.Equal("1.5", svg.GetAttribute("stroke-width"));
        Assert.Equal("outer inner", svg.GetAttribute("class"));
    }

    [Fact]
    public void AbsoluteStroke_UsesTheDefaultSize()
    {
        var svg = Defaults(p => p
                .Add(x => x.Size, "48")
                .Add(x => x.AbsoluteStroke, true)
                .AddChildContent<ZapIcon>())
            .Find("svg");

        Assert.Equal("1", svg.GetAttribute("stroke-width")); // 2 × 24 / 48
    }

    [Fact]
    public void DispatcherAndFactory_FollowTheDefaults()
    {
        RenderFragment content = b =>
        {
            b.OpenComponent<ShellIcon>(0);
            b.AddAttribute(1, nameof(ShellIcon.Name), "zap");
            b.CloseComponent();
            b.AddContent(2, Icon.ChevronRight());
        };

        var svgs = Defaults(p => p.Add(x => x.Size, "16").AddChildContent(content)).FindAll("svg");

        Assert.Equal(2, svgs.Count);
        Assert.All(svgs, svg => Assert.Equal("16", svg.GetAttribute("width")));
    }

    [Fact]
    public void ChangingTheDefaults_ReRendersTheIcons()
    {
        var cut = Defaults(p => p.Add(x => x.Size, "16").AddChildContent<ZapIcon>());
        Assert.Equal("16", cut.Find("svg").GetAttribute("width"));

        cut.SetParametersAndRender(p => p.Add(x => x.Size, "32"));
        Assert.Equal("32", cut.Find("svg").GetAttribute("width"));
    }

    [Fact]
    public void WithoutDefaults_IconsKeepTheBuiltInValues()
    {
        var svg = _ctx.RenderComponent<ZapIcon>().Find("svg");

        Assert.Equal("24", svg.GetAttribute("width"));
        Assert.Equal("2", svg.GetAttribute("stroke-width"));
        Assert.Null(svg.GetAttribute("class"));
    }
}
