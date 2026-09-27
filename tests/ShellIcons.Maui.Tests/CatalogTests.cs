namespace ShellIcons.Maui.Tests;

public class CatalogTests
{
    [Fact]
    public void Enum_HasEveryIcon_PlusNone()
    {
        Assert.True(IconCatalog.Count > 1500, $"catalog has {IconCatalog.Count} icons");
        Assert.Equal(IconCatalog.Count + 1, Enum.GetValues<IconName>().Length);
        Assert.Equal(0, (int)IconName.None);
    }

    [Theory]
    [InlineData("chevron-right", IconName.ChevronRight)]
    [InlineData("ChevronRight", IconName.ChevronRight)]
    [InlineData("CHEVRON-RIGHT", IconName.ChevronRight)]
    [InlineData("  zap ", IconName.Zap)]
    [InlineData("home", IconName.House)]          // Lucide alias: home was renamed to house
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
