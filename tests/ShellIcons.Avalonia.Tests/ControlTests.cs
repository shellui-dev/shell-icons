using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ShellIcons.Avalonia.Tests;

public class ControlTests
{
    [AvaloniaFact]
    public void TypedControl_KnowsItsName_AndHasGeometry()
    {
        var icon = new Icons.ChevronRight();

        Assert.Equal(IconName.ChevronRight, icon.IconName);
        Assert.NotNull(icon.StrokeGeometry);
        Assert.Null(icon.FillGeometry);
    }

    [AvaloniaFact]
    public void Dispatcher_LoadsByKind_AndReloadsOnChange()
    {
        var icon = new Icon { Kind = IconName.House };
        Assert.Equal(IconName.House, icon.IconName);
        var house = icon.StrokeGeometry;

        icon.Kind = IconName.ChevronRight;
        Assert.Equal(IconName.ChevronRight, icon.IconName);
        Assert.NotSame(house, icon.StrokeGeometry);
    }

    [AvaloniaFact]
    public void Dispatcher_None_DrawsNothing()
    {
        var icon = new Icon();
        Assert.Null(icon.StrokeGeometry);

        icon.Kind = IconName.Zap;
        icon.Kind = IconName.None;
        Assert.Null(icon.StrokeGeometry);
    }

    [AvaloniaFact]
    public void TypedAndDispatcher_ShareTheSameCachedGeometry()
    {
        Assert.Same(new Icons.Zap().StrokeGeometry, new Icon { Kind = IconName.Zap }.StrokeGeometry);
    }

    [AvaloniaFact]
    public void Defaults_MatchTheOtherTargets()
    {
        var icon = new Icons.Zap();

        Assert.Equal(24, icon.Size);
        Assert.Equal(2, icon.StrokeThickness);
        Assert.Equal(2, icon.EffectiveStrokeThickness);
        Assert.False(icon.IsHitTestVisible);
        Assert.Equal(HorizontalAlignment.Center, icon.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Center, icon.VerticalAlignment);
    }

    [AvaloniaFact]
    public void Size_DrivesLayout_AndScalesTheStroke()
    {
        var icon = new Icons.Zap { Size = 16 };
        icon.Measure(global::Avalonia.Size.Infinity);

        Assert.Equal(new global::Avalonia.Size(16, 16), icon.DesiredSize);
        Assert.Equal(2 * 16d / 24, icon.EffectiveStrokeThickness, 6);
    }

    [AvaloniaFact]
    public void AbsoluteStroke_KeepsStrokeWidthAtAnySize()
    {
        var icon = new Icons.Zap { Size = 48, StrokeThickness = 1.5, AbsoluteStroke = true };
        Assert.Equal(1.5, icon.EffectiveStrokeThickness);
    }

    [AvaloniaFact]
    public void Foreground_IsInheritedFromTheParent()
    {
        var icon = new Icons.Zap();
        var window = new Window { Content = new Button { Foreground = Brushes.Crimson, Content = icon } };
        window.Show();

        Assert.Same(Brushes.Crimson, icon.Foreground);

        icon.Foreground = Brushes.Blue;
        Assert.Same(Brushes.Blue, icon.Foreground);
        window.Close();
    }

    [AvaloniaFact]
    public void FilledIcon_HasASeparateFillGeometry()
    {
        var name = Enum.GetValues<IconName>().First(n => IconData.Get(n).Fill is not null);
        var icon = new Icon { Kind = name };
        Assert.NotNull(icon.FillGeometry);

        icon.Kind = IconName.ChevronRight;
        Assert.Null(icon.FillGeometry);
    }

    [AvaloniaFact]
    public void Title_MakesTheIconAccessible()
    {
        var icon = new Icons.X();
        Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(icon));

        icon.Title = "Close dialog";
        Assert.Equal("Close dialog", AutomationProperties.GetName(icon));
        Assert.Equal(AccessibilityView.Content, AutomationProperties.GetAccessibilityView(icon));

        var peer = ControlAutomationPeer.CreatePeerForElement(icon);
        Assert.Equal(AutomationControlType.Image, peer.GetAutomationControlType());
        Assert.Equal("Close dialog", peer.GetName());
    }
}
