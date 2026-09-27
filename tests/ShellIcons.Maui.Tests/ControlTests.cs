using Microsoft.Maui.Controls.Shapes;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace ShellIcons.Maui.Tests;

public class ControlTests
{
    [Fact]
    public void TypedControl_KnowsItsName_AndDrawsOnePath()
    {
        var icon = new Icons.ChevronRight();

        Assert.Equal(IconName.ChevronRight, icon.IconName);
        var path = Assert.IsType<Path>(icon.Content);
        var geometry = Assert.IsType<PathGeometry>(path.Data);
        Assert.Single(geometry.Figures);
    }

    [Fact]
    public void Dispatcher_LoadsByName_AndReloadsOnChange()
    {
        var icon = new Icon { Name = IconName.House };
        Assert.Equal(IconName.House, icon.IconName);
        Assert.Equal(2, ((PathGeometry)((Path)icon.Content).Data).Figures.Count);

        icon.Name = IconName.ChevronRight;
        Assert.Equal(IconName.ChevronRight, icon.IconName);
        Assert.Single(((PathGeometry)((Path)icon.Content).Data).Figures);
    }

    [Fact]
    public void Dispatcher_None_DrawsNothing()
    {
        var icon = new Icon();
        Assert.Null(((Path)icon.Content).Data);
    }

    [Fact]
    public void TypedAndDispatcher_ShareTheSameCachedGeometry()
    {
        var typed = (Path)new Icons.Zap().Content;
        var dispatched = (Path)new Icon { Name = IconName.Zap }.Content;

        Assert.Same(typed.Data, dispatched.Data);
    }

    [Fact]
    public void Defaults_MatchTheBlazorTarget()
    {
        var icon = new Icons.Zap();
        var path = (Path)icon.Content;

        Assert.Equal(24, icon.Size);
        Assert.Equal(2, icon.StrokeThickness);
        Assert.Equal(24, icon.WidthRequest);
        Assert.True(icon.InputTransparent);
        Assert.Equal(LayoutOptions.Center, icon.HorizontalOptions);
        Assert.Equal(Stretch.None, path.Aspect);
        Assert.Equal(PenLineCap.Round, path.StrokeLineCap);
        Assert.Equal(PenLineJoin.Round, path.StrokeLineJoin);
    }

    [Fact]
    public void Size_ScalesGeometryAndStroke()
    {
        var icon = new Icons.Zap { Size = 16 };
        var path = (Path)icon.Content;

        Assert.Equal(16, icon.WidthRequest);
        var scale = Assert.IsType<ScaleTransform>(path.RenderTransform);
        Assert.Equal(16d / 24, scale.ScaleX, 6);
        Assert.Equal(2 * 16d / 24, path.StrokeThickness, 6);
    }

    [Fact]
    public void AbsoluteStroke_KeepsStrokeWidthAtAnySize()
    {
        var icon = new Icons.Zap { Size = 48, StrokeThickness = 1.5, AbsoluteStroke = true };
        Assert.Equal(1.5, ((Path)icon.Content).StrokeThickness);
    }

    [Fact]
    public void Color_SetsStroke()
    {
        var icon = new Icons.Zap { Color = Colors.Crimson };
        var brush = Assert.IsType<SolidColorBrush>(((Path)icon.Content).Stroke);
        Assert.Equal(Colors.Crimson, brush.Color);
    }

    [Fact]
    public void FilledIcon_UsesASecondPath_ThatIsFilledAndStroked()
    {
        var name = Enum.GetValues<IconName>().First(n => IconData.Get(n).Fill is not null);
        var icon = new Icon { Name = name, Color = Colors.Blue };

        var grid = Assert.IsType<Grid>(icon.Content);
        Assert.Equal(2, grid.Children.Count);
        var fill = (Path)grid.Children[1];
        Assert.Equal(Colors.Blue, Assert.IsType<SolidColorBrush>(fill.Fill).Color);
        Assert.Equal(Colors.Blue, Assert.IsType<SolidColorBrush>(fill.Stroke).Color);

        // Switching to a stroke-only icon collapses back to a single Path.
        icon.Name = IconName.ChevronRight;
        Assert.IsType<Path>(icon.Content);
    }

    [Fact]
    public void Title_MakesTheIconAccessible()
    {
        var icon = new Icons.X();
        Assert.False(AutomationProperties.GetIsInAccessibleTree(icon));

        icon.Title = "Close dialog";
        Assert.Equal("Close dialog", SemanticProperties.GetDescription(icon));
        Assert.True(AutomationProperties.GetIsInAccessibleTree(icon));
    }

    [Fact]
    public void EveryIcon_HasATypedControl()
    {
        var typed = typeof(IconView).Assembly.GetTypes()
            .Where(t => t.Namespace == "ShellIcons.Maui.Icons" && t.IsSubclassOf(typeof(IconView)))
            .ToList();

        Assert.Equal(IconCatalog.Count, typed.Count);
    }
}
