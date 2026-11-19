namespace ShellIcons.Maui.Tests;

public class CatalogTests
{
    [Fact]
    public void Enum_HasEveryIcon_PlusNone()
    {
        Assert.True(IconCatalog.Count > 2000, $"catalog has {IconCatalog.Count} icons");
        Assert.Equal(IconCatalog.Count, IconCatalog.All.Count);
        Assert.Equal(0, (int)IconName.None);

        // Icons are 1..Count; former names come after them.
        Assert.Equal(Enumerable.Range(1, IconCatalog.Count), IconCatalog.All.Select(i => (int)i.Name));
    }

#pragma warning disable CS0618 // former names are [Obsolete]
    [Fact]
    public void FormerNames_ResolveToTheRenamedIcon()
    {
        Assert.True((int)IconName.Trash2 > IconCatalog.Count);
        Assert.Equal(IconName.Trash, IconData.Resolve(IconName.Trash2));
        Assert.Equal(IconName.Funnel, IconCatalog.Get(IconName.Filter).Name);
        Assert.Equal(IconData.Get(IconName.CircleQuestionMark), IconData.Get(IconName.CircleHelp));
        Assert.DoesNotContain(IconCatalog.All, i => i.Name == IconName.Trash2);
    }

    [Fact]
    public void FormerNames_AreObsolete_AndNameTheReplacement()
    {
        var field = typeof(IconName).GetField(nameof(IconName.Trash2))!;
        var obsolete = Assert.Single(field.GetCustomAttributes(typeof(ObsoleteAttribute), false).Cast<ObsoleteAttribute>());
        Assert.Equal("Lucide renamed 'trash-2' to 'trash'. Use IconName.Trash.", obsolete.Message);
    }
#pragma warning restore CS0618

    [Fact]
    public void Tags_ExpandLucideDirectives()
    {
        // alarm-clock-check: "$extends:alarm-clock" + "$group:check" in the sidecar.
        var info = IconCatalog.Get(IconName.AlarmClockCheck);
        Assert.Contains("done", info.Tags);
        Assert.Contains("wake-up", info.Tags);
        Assert.Contains(IconCatalog.Get(IconName.AlarmClock).Tags[0], info.Tags);
        Assert.NotEmpty(info.Categories);

        Assert.DoesNotContain(IconCatalog.All, i => i.Tags.Concat(i.Categories).Any(t => t.StartsWith("$extends:") || t.StartsWith("$group:")));
    }

    [Theory]
    [InlineData("chevron-right", IconName.ChevronRight)]
    [InlineData("ChevronRight", IconName.ChevronRight)]
    [InlineData("CHEVRON-RIGHT", IconName.ChevronRight)]
    [InlineData("  zap ", IconName.Zap)]
    [InlineData("home", IconName.House)]          // Lucide alias: home was renamed to house
    [InlineData("trash-2", IconName.Trash)]       // renamed in Lucide 1.x
    [InlineData("Trash2", IconName.Trash)]
    [InlineData("filter", IconName.Funnel)]
    [InlineData("letters", IconName.Letters)]     // letters and letter-s differ only by case
    [InlineData("LetterS", IconName.LetterS)]
    [InlineData("Letters", IconName.Letters)]
    public void TryParse_ResolvesIdsNamesAndAliases(string input, IconName expected)
    {
        Assert.True(IconCatalog.TryParse(input, out var name));
        Assert.Equal(expected, name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-icon")]
    public void TryParse_Unknown_ReturnsFalse(string? input)
    {
        Assert.False(IconCatalog.TryParse(input, out var name));
        Assert.Equal(IconName.None, name);
    }

    [Fact]
    public void TryParse_CanonicalIdBeatsAnotherIconsAlias()
    {
        foreach (var info in IconCatalog.All)
        {
            Assert.True(IconCatalog.TryParse(info.Id, out var name));
            Assert.Equal(info.Name, name);
        }
    }

    [Fact]
    public void Get_ReturnsSidecarMetadata()
    {
        var house = IconCatalog.Get(IconName.House);

        Assert.Equal("house", house.Id);
        Assert.Equal("lucide", house.Source);
        Assert.Contains("home", house.Aliases);
        Assert.Contains("building", house.Tags);
        Assert.NotEmpty(house.Categories);
    }

    [Fact]
    public void Get_None_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IconCatalog.Get(IconName.None));
    }

    [Fact]
    public void Search_RanksIdMatchesFirst_ThenOtherMatches()
    {
        var results = IconCatalog.Search("cart").ToList();

        Assert.Contains(results, r => r.Name == IconName.ShoppingCart);
        var firstNonIdMatch = results.FindIndex(r => !r.Id.Contains("cart"));
        var lastIdMatch = results.FindLastIndex(r => r.Id.Contains("cart"));
        Assert.True(firstNonIdMatch == -1 || lastIdMatch < firstNonIdMatch);
    }
}
