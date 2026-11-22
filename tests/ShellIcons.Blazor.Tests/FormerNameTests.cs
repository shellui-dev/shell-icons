#pragma warning disable CS0618 // former names are [Obsolete]; that's what these tests use

using Bunit;
using ShellIcons.Icons;

namespace ShellIcons.Blazor.Tests;

// Lucide renames icons between versions; the old names stay as [Obsolete] aliases of the new icon.
public class FormerNameTests : IDisposable
{
    private readonly TestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    [Fact]
    public void TypedComponent_UnderFormerName_RendersTheRenamedIcon()
    {
        Assert.Equal(_ctx.RenderComponent<Trash>().Markup, _ctx.RenderComponent<Trash2>().Markup);
        Assert.Equal(_ctx.RenderComponent<Funnel>().Markup, _ctx.RenderComponent<Filter>().Markup);
    }

    [Fact]
    public void SuffixedComponent_UnderFormerName_RendersTheRenamedIcon()
    {
        Assert.Equal(_ctx.RenderComponent<CircleQuestionMarkIcon>().Markup, _ctx.RenderComponent<CircleHelpIcon>().Markup);
    }

    [Fact]
    public void Factory_UnderFormerName_RendersTheRenamedIcon()
    {
        Assert.Equal(_ctx.Render(Icon.Trash()).Markup, _ctx.Render(Icon.Trash2()).Markup);
    }

    [Theory]
    [InlineData(typeof(Trash2), "Use Trash.")]
    [InlineData(typeof(Trash2Icon), "Use TrashIcon.")]
    public void FormerNames_AreObsolete_AndNameTheReplacement(System.Type type, string expected)
    {
        var obsolete = Assert.Single(type.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false).Cast<ObsoleteAttribute>());
        Assert.Contains("'trash-2'", obsolete.Message);
        Assert.EndsWith(expected, obsolete.Message);
    }

    [Theory]
    [InlineData("trash-2", "trash")]
    [InlineData("filter", "funnel")]
    [InlineData("home", "house")]
    [InlineData("arrow-down-01", "arrow-down-0-1")] // spelling variant with no C# name of its own
    public void Dispatcher_AcceptsFormerNames(string former, string current)
    {
        var expected = _ctx.RenderComponent<ShellIcon>(p => p.Add(x => x.Name, current)).Markup;
        var actual = _ctx.RenderComponent<ShellIcon>(p => p.Add(x => x.Name, former)).Markup;

        Assert.Equal(expected, actual);
        Assert.DoesNotContain(former, ShellIcon.Names);
    }

    [Fact]
    public void RemovedBrandIcons_AreGone()
    {
        var assembly = typeof(IconCore).Assembly;
        Assert.Null(assembly.GetType("ShellIcons.Icons.Github"));
        Assert.Null(assembly.GetType("ShellIcons.GithubIcon"));
        Assert.DoesNotContain("twitter", ShellIcon.Names);
    }

    [Fact]
    public void LetterS_AndLetters_AreDifferentIcons()
    {
        // The names differ only by case; both must be generated.
        Assert.NotEqual(_ctx.RenderComponent<LetterS>().Markup, _ctx.RenderComponent<Letters>().Markup);
    }
}
