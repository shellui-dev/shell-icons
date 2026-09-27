namespace ShellIcons.Maui.Tests;

public class XamlTests
{
    [Fact]
    public void Xmlns_ResolvesDispatcherAndTypedControls()
    {
        var view = new XamlFixture();

        Assert.Equal(IconName.Search, view.Dispatched.Name);   // string → enum in XAML
        Assert.Equal(IconName.Search, view.Dispatched.IconName);
        Assert.Equal(16, view.Dispatched.Size);
        Assert.Equal(Colors.Crimson, view.Dispatched.Color);

        Assert.Equal(IconName.ChevronRight, view.Typed.IconName);
        Assert.Equal(1.5, view.Typed.EffectiveStrokeThickness);
    }

    [Fact]
    public void TypedIconNamedImage_CoexistsWithMauiImage()
    {
        var view = new XamlFixture();

        Assert.IsType<Icons.Image>(view.TypedImageIcon);
        Assert.IsType<Microsoft.Maui.Controls.Image>(view.MauiImage);
    }
}
