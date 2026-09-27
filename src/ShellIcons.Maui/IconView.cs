using Microsoft.Maui.Controls.Shapes;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace ShellIcons.Maui;

/// <summary>
/// Base control for every icon: one native <see cref="Path"/> on Lucide's 24×24 grid, plus a
/// second filled path only for icons with <c>fill="currentColor"</c> shapes.
/// </summary>
public class IconView : ContentView
{
    private const double GridUnits = 24;

    /// <summary>Width and height in device-independent units. Default 24.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(IconView), GridUnits, propertyChanged: OnMetricsChanged);

    /// <summary>Stroke width on the 24-unit grid (Lucide's weight). Default 2.</summary>
    public static readonly BindableProperty StrokeThicknessProperty = BindableProperty.Create(
        nameof(StrokeThickness), typeof(double), typeof(IconView), 2d, propertyChanged: OnMetricsChanged);

    /// <summary>When true the rendered stroke stays <see cref="StrokeThickness"/> wide at any size.</summary>
    public static readonly BindableProperty AbsoluteStrokeProperty = BindableProperty.Create(
        nameof(AbsoluteStroke), typeof(bool), typeof(IconView), false, propertyChanged: OnMetricsChanged);

    /// <summary>Stroke (and fill) color. When null the icon is black in light theme and white in dark.</summary>
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color), typeof(Color), typeof(IconView), null, propertyChanged: (b, _, _) => ((IconView)b).ApplyColor());

    /// <summary>Accessible description. When null the icon is decorative and hidden from screen readers.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(IconView), null, propertyChanged: (b, _, _) => ((IconView)b).ApplyTitle());

    private static readonly Brush LightThemeBrush = new SolidColorBrush(Colors.Black);
    private static readonly Brush DarkThemeBrush = new SolidColorBrush(Colors.White);

    private readonly Path _stroke;
    private Path? _fill;

    /// <summary>Creates an empty icon; call <see cref="SetData"/> to give it a shape.</summary>
    protected IconView()
    {
        _stroke = CreatePath();
        Content = _stroke;

        // Icons are decoration for hit testing: a tap lands on the button or row hosting them.
        InputTransparent = true;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;

        ApplyMetrics();
        ApplyColor();
        ApplyTitle();
    }

    /// <summary>Creates an icon with fixed path data (used by the generated typed controls).</summary>
    protected IconView(IconName iconName, string strokeData, string? fillData) : this()
    {
        SetData(iconName, strokeData, fillData);
    }

    /// <summary>The icon this control currently shows.</summary>
    public IconName IconName { get; private set; }

    /// <inheritdoc cref="SizeProperty"/>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <inheritdoc cref="StrokeThicknessProperty"/>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <inheritdoc cref="AbsoluteStrokeProperty"/>
    public bool AbsoluteStroke
    {
        get => (bool)GetValue(AbsoluteStrokeProperty);
        set => SetValue(AbsoluteStrokeProperty, value);
    }

    /// <inheritdoc cref="ColorProperty"/>
    public Color? Color
    {
        get => (Color?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <inheritdoc cref="TitleProperty"/>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The rendered stroke thickness after size scaling (what the native path draws).</summary>
    public double EffectiveStrokeThickness =>
        AbsoluteStroke ? StrokeThickness : StrokeThickness * (Size / GridUnits);

    /// <summary>Replaces the drawn shapes.</summary>
    protected void SetData(IconName iconName, string strokeData, string? fillData)
    {
        IconName = iconName;
        _stroke.Data = IconGeometry.Get(strokeData);

        if (fillData is null)
        {
            if (_fill is not null)
            {
                _fill = null;
                Content = _stroke;
            }
            return;
        }

        if (_fill is null)
        {
            _fill = CreatePath();
            // The rare filled shapes are drawn on top, filled *and* stroked like the SVG.
            Content = new Grid { Children = { _stroke, _fill } };
            ApplyMetrics();
            ApplyColor();
        }
        _fill.Data = IconGeometry.Get(fillData);
    }

    private static Path CreatePath() => new()
    {
        // Aspect=None keeps icons on the shared grid; Uniform would scale each to its own bounds.
        Aspect = Stretch.None,
        StrokeLineCap = PenLineCap.Round,
        StrokeLineJoin = PenLineJoin.Round,
        InputTransparent = true,
    };

    private static void OnMetricsChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((IconView)bindable).ApplyMetrics();

    private void ApplyMetrics()
    {
        var size = Size;
        var scale = size / GridUnits;
        var thickness = EffectiveStrokeThickness;

        WidthRequest = size;
        HeightRequest = size;

        foreach (var path in Paths())
        {
            path.WidthRequest = size;
            path.HeightRequest = size;
            // The transform scales the geometry, not the pen, so the stroke is scaled separately.
            path.RenderTransform = new ScaleTransform(scale, scale);
            path.StrokeThickness = thickness;
        }
    }

    private void ApplyColor()
    {
        foreach (var path in Paths())
        {
            var isFill = ReferenceEquals(path, _fill);
            path.RemoveBinding(Shape.StrokeProperty);
            if (isFill) path.RemoveBinding(Shape.FillProperty);

            if (Color is { } color)
            {
                var brush = new SolidColorBrush(color);
                path.Stroke = brush;
                if (isFill) path.Fill = brush;
            }
            else
            {
                path.SetAppTheme<Brush>(Shape.StrokeProperty, LightThemeBrush, DarkThemeBrush);
                if (isFill) path.SetAppTheme<Brush>(Shape.FillProperty, LightThemeBrush, DarkThemeBrush);
            }
        }
    }

    private void ApplyTitle()
    {
        var title = Title;
        SemanticProperties.SetDescription(this, title);
        AutomationProperties.SetIsInAccessibleTree(this, title is not null);
    }

    private IEnumerable<Path> Paths()
    {
        yield return _stroke;
        if (_fill is not null) yield return _fill;
    }
}
