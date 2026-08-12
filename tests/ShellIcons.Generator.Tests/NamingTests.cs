using ShellIcons.Generator;

namespace ShellIcons.Generator.Tests;

public class NamingTests
{
    [Theory]
    [InlineData("chevron-right", "ChevronRight")]
    [InlineData("zap", "Zap")]
    [InlineData("x", "X")]
    [InlineData("circle-alert", "CircleAlert")]
    [InlineData("triangle-alert", "TriangleAlert")]
    [InlineData("a-arrow-down", "AArrowDown")]
    [InlineData("bluetooth-off", "BluetoothOff")]
    [InlineData("wifi-high", "WifiHigh")]
    public void KebabToPascal_KnownCases(string input, string expected)
    {
        Assert.Equal(expected, Naming.KebabToPascal(input));
    }

    [Fact]
    public void KebabToPascal_Empty_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, Naming.KebabToPascal(""));
    }

    [Fact]
    public void KebabToPascal_SingleChar_Capitalizes()
    {
        Assert.Equal("A", Naming.KebabToPascal("a"));
    }

    [Fact]
    public void KebabToPascal_Underscore_TreatedAsSeparator()
    {
        Assert.Equal("MyIcon", Naming.KebabToPascal("my_icon"));
    }

    [Fact]
    public void KebabToPascal_LeadingHyphen_SkipsEmptySegment()
    {
        Assert.Equal("Foo", Naming.KebabToPascal("-foo"));
    }

    [Fact]
    public void KebabToPascal_ConsecutiveHyphens_SkipsEmpty()
    {
        Assert.Equal("FooBar", Naming.KebabToPascal("foo--bar"));
    }
}
