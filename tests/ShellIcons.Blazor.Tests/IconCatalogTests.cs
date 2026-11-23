using System.Text.Json;

namespace ShellIcons.Blazor.Tests;

public class IconCatalogTests
{
    [Fact]
    public void All_ListsEveryIcon_TheDispatcherKnows()
    {
        Assert.Equal(IconCatalog.Count, IconCatalog.All.Count);
        Assert.Equal(ShellIcon.Names.OrderBy(n => n, StringComparer.Ordinal), IconCatalog.All.Select(i => i.Id));
    }

    [Fact]
    public void Names_MatchTheComponents()
    {
        var info = IconCatalog.Get("chevron-right");

        Assert.Equal("ChevronRight", info.Name);
        Assert.NotNull(typeof(IconCore).Assembly.GetType("ShellIcons.Icons." + info.Name));
        Assert.NotNull(typeof(IconCore).Assembly.GetType("ShellIcons." + info.Name + "Icon"));
    }

    [Theory]
    [InlineData("chevron-right", "chevron-right")]
    [InlineData("ChevronRight", "chevron-right")]
    [InlineData("  CHEVRON-RIGHT ", "chevron-right")]
    [InlineData("trash-2", "trash")]     // former names
    [InlineData("Trash2", "trash")]
    [InlineData("home", "house")]
    [InlineData("LetterS", "letter-s")]  // exact case first: Letters is another icon
    [InlineData("Letters", "letters")]
    public void TryParse_ResolvesIdsNamesAndFormerNames(string input, string expected)
    {
        Assert.True(IconCatalog.TryParse(input, out var id));
        Assert.Equal(expected, id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("github")] // removed from Lucide
    public void TryParse_Unknown_ReturnsFalse(string? input)
    {
        Assert.False(IconCatalog.TryParse(input, out var id));
        Assert.Equal(string.Empty, id);
        Assert.False(IconCatalog.TryGet(input, out _));
    }

    [Fact]
    public void Get_Unknown_Throws()
    {
        Assert.Throws<ArgumentException>(() => IconCatalog.Get("not-an-icon"));
    }

    [Fact]
    public void Metadata_ComesFromTheSidecars_WithLucideDirectivesExpanded()
    {
        var house = IconCatalog.Get("house");
        Assert.Equal("lucide", house.Source);
        Assert.Contains("home", house.Aliases);
        Assert.Contains("building", house.Tags);
        Assert.NotEmpty(house.Categories);

        Assert.Contains("done", IconCatalog.Get("alarm-clock-check").Tags); // from "$group:check"
        Assert.DoesNotContain(IconCatalog.All, i => i.Tags.Any(t => t.StartsWith("$group:") || t.StartsWith("$extends:")));
    }

    [Fact]
    public void Search_RanksIdMatchesFirst()
    {
        var results = IconCatalog.Search("cart").ToList();

        Assert.Contains(results, r => r.Id == "shopping-cart");
        var firstNonIdMatch = results.FindIndex(r => !r.Id.Contains("cart"));
        var lastIdMatch = results.FindLastIndex(r => r.Id.Contains("cart"));
        Assert.True(firstNonIdMatch == -1 || lastIdMatch < firstNonIdMatch);
    }

    [Fact]
    public void Search_FindsIconsByTag()
    {
        Assert.Contains(IconCatalog.Search("done"), i => i.Id == "circle-check");
    }

    [Fact]
    public void ToJson_ListsEveryIcon()
    {
        using var doc = JsonDocument.Parse(IconCatalog.ToJson());
        var icons = doc.RootElement.EnumerateArray().ToList();

        Assert.Equal(IconCatalog.Count, icons.Count);
        var trash = icons.Single(i => i.GetProperty("id").GetString() == "trash");
        Assert.Equal("Trash", trash.GetProperty("name").GetString());
        Assert.Contains("trash-2", trash.GetProperty("aliases").EnumerateArray().Select(a => a.GetString()));
    }

    // The package serves wwwroot/icons.json as _content/ShellIcons.Blazor/icons.json; it must match the catalog.
    [Fact]
    public void IconsJsonAsset_MatchesTheCatalog()
    {
        var path = Path.Combine(RepoRoot(), "src", "ShellIcons.Blazor", "wwwroot", "icons.json");
        var expected = IconCatalog.ToJson();

        if (Environment.GetEnvironmentVariable("SHELLICONS_UPDATE_ICONS_JSON") == "1")
            File.WriteAllText(path, expected);

        Assert.True(File.Exists(path) && File.ReadAllText(path) == expected,
            "icons.json is out of date. Refresh it with: $env:SHELLICONS_UPDATE_ICONS_JSON=1; dotnet test tests/ShellIcons.Blazor.Tests");
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LUCIDE_VERSION.txt"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Repo root not found.");
    }
}
