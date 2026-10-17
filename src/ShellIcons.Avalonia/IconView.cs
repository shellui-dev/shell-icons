using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;

namespace ShellIcons.Avalonia;

/// <summary>
/// Base control for every icon: draws the icon's geometry on Lucide's 24×24 grid, scaled to
/// <see cref="Size"/>. Shapes with <c>fill="currentColor"</c> are filled as well as stroked.
/// </summary>
public class IconView : Control
{
    private const double GridUnits = 24;

    /// <summary>Width and height in device-independent units. Default 24.</summary>
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<IconView, double>(nameof(Size), GridUnits);

    /// <summary>Stroke width on the 24-unit grid (Lucide's weight). Default 2.</summary>
    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<IconView, double>(nameof(StrokeThickness), 2d);

    /// <summary>When true the rendered stroke stays <see cref="StrokeThickness"/> wide at any size.</summary>
    public static readonly StyledProperty<bool> AbsoluteStrokeProperty =
        AvaloniaProperty.Register<IconView, bool>(nameof(AbsoluteStroke));

    /// <summary>Stroke (and fill) brush. Inherited, so an icon takes its parent's text color by default.</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<IconView>();

    /// <summary>Accessible name. When null the icon is decorative and hidden from screen readers.</summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<IconView, string?>(nameof(Title));

    private Geometry? _stroke;
    private Geometry? _fill;

    static IconView()
    {
        AffectsMeasure<IconView>(SizeProperty);
        AffectsRender<IconView>(SizeProperty, StrokeThicknessProperty, AbsoluteStrokeProperty, ForegroundProperty);

        // Icons are decoration for hit testing: a click lands on the button or row hosting them.
        IsHitTestVisibleProperty.OverrideDefaultValue<IconView>(false);
        HorizontalAlignmentProperty.OverrideDefaultValue<IconView>(HorizontalAlignment.Center);
        VerticalAlignmentProperty.OverrideDefaultValue<IconView>(VerticalAlignment.Center);
        AutomationProperties.AccessibilityViewProperty.OverrideDefaultValue<IconView>(AccessibilityView.Raw);

        TitleProperty.Changed.AddClassHandler<IconView>((icon, _) => icon.ApplyTitle());
    }

    /// <summary>Creates an empty icon; call <see cref="SetData"/> to give it a shape.</summary>
    protected IconView()
    {
    }

    /// <summary>Creates an icon with fixed path data (used by the generated typed controls).</summary>
    protected IconView(IconName iconName, string strokeData, string? fillData)
    {
        SetData(iconName, strokeData, fillData);
    }

    /// <summary>The icon this control currently shows.</summary>
    public IconName IconName { get; private set; }

    /// <inheritdoc cref="SizeProperty"/>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <inheritdoc cref="StrokeThicknessProperty"/>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <inheritdoc cref="AbsoluteStrokeProperty"/>
    public bool AbsoluteStroke
    {
        get => GetValue(AbsoluteStrokeProperty);
        set => SetValue(AbsoluteStrokeProperty, value);
    }

    /// <inheritdoc cref="ForegroundProperty"/>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <inheritdoc cref="TitleProperty"/>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The rendered stroke thickness after size scaling.</summary>
    public double EffectiveStrokeThickness =>
        AbsoluteStroke ? StrokeThickness : StrokeThickness * (Size / GridUnits);

    internal Geometry? StrokeGeometry => _stroke;

    internal Geometry? FillGeometry => _fill;

    /// <summary>Replaces the drawn shapes.</summary>
    protected void SetData(IconName iconName, string strokeData, string? fillData)
    {
        IconName = iconName;
        _stroke = IconGeometry.Get(strokeData);
        _fill = IconGeometry.Get(fillData);
        InvalidateVisual();
    }

    /// <inheritdoc/>
    protected override global::Avalonia.Size MeasureOverride(global::Avalonia.Size availableSize)
    {
        var size = Math.Max(0, Size);
        return new global::Avalonia.Size(size, size);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        var size = Size;
        if (size <= 0 || Foreground is not { } brush || (_stroke is null && _fill is null)) return;

        var scale = size / GridUnits;

        // The transform scales the pen with the geometry, so the pen is sized in grid units.
        var pen = new Pen(brush, EffectiveStrokeThickness / scale, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);

        using (context.PushTransform(Matrix.CreateScale(scale, scale)))
        {
            if (_stroke is not null) context.DrawGeometry(null, pen, _stroke);
            // The rare filled shapes are drawn on top, filled *and* stroked like the SVG.
            if (_fill is not null) context.DrawGeometry(brush, pen, _fill);
        }
    }

    /// <inheritdoc/>
    protected override AutomationPeer OnCreateAutomationPeer() => new IconAutomationPeer(this);

    private void ApplyTitle()
    {
        var title = Title;
        AutomationProperties.SetName(this, title);
        AutomationProperties.SetAccessibilityView(this, title is null ? AccessibilityView.Raw : AccessibilityView.Content);
    }

    private sealed class IconAutomationPeer(IconView owner) : ControlAutomationPeer(owner)
    {
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Image;
    }
}
