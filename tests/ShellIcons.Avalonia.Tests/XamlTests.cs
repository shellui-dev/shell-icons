using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ShellIcons.Avalonia.Tests;

public class XamlTests
{
    // Covers the xmlns, string → IconName conversion, and icons:Image next to Avalonia's Image.
    private const string Xaml = """
        <StackPanel xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:icons="https://shellicons.dev/avalonia">
            <icons:Icon Kind="Search" Size="16" Foreground="Crimson" />
            <icons:ChevronRight StrokeThickness="1.5" AbsoluteStroke="True" />
            <icons:Image />
            <Image />
        </StackPanel>
        """;

    [AvaloniaFact]
    public void Xmlns_ResolvesDispatcherAndTypedControls()
    {
        var panel = AvaloniaRuntimeXamlLoader.Parse<StackPanel>(Xaml, typeof(IconView).Assembly);

        var dispatched = Assert.IsType<Icon>(panel.Children[0]);
        Assert.Equal(IconName.Search, dispatched.Kind);
        Assert.Equal(IconName.Search, dispatched.IconName);
        Assert.Equal(16, dispatched.Size);
        Assert.Equal(Colors.Crimson, Assert.IsAssignableFrom<ISolidColorBrush>(dispatched.Foreground).Color);

        var typed = Assert.IsType<Icons.ChevronRight>(panel.Children[1]);
        Assert.Equal(1.5, typed.EffectiveStrokeThickness);
    }

    [AvaloniaFact]
    public void TypedIconNamedImage_CoexistsWithAvaloniaImage()
    {
        var panel = AvaloniaRuntimeXamlLoader.Parse<StackPanel>(Xaml, typeof(IconView).Assembly);

        Assert.IsType<Icons.Image>(panel.Children[2]);
        Assert.IsType<global::Avalonia.Controls.Image>(panel.Children[3]);
    }
}
