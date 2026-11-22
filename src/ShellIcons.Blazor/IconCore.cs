using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ShellIcons;

/// <summary>
/// Base class for every ShellIcons Blazor icon. Owns the &lt;svg&gt; root emission,
/// prop plumbing, and the accessibility path. Icon subclasses only supply the
/// inner shape markup via <see cref="EmitChildren"/>.
/// </summary>
public abstract class IconCore : ComponentBase
{
    private static int _idCounter;
    private string? _titleId;

    // Which of the defaultable parameters the icon set itself; the rest come from an enclosing IconDefaults.
    private bool _sizeSet;
    private bool _strokeWidthSet;
    private bool _absoluteStrokeSet;

    /// <summary>Width and height of the rendered SVG. Any valid SVG size string: <c>"24"</c>, <c>"1.5em"</c>, <c>"100%"</c>. Default <c>"24"</c>.</summary>
    [Parameter] public string Size { get; set; } = "24";

    /// <summary>Stroke width of the icon. Default 2 (matches Lucide).</summary>
    [Parameter] public double StrokeWidth { get; set; } = 2;

    /// <summary>When true, keeps the stroke width consistent regardless of <see cref="Size"/> (Lucide's <c>absoluteStrokeWidth</c>).</summary>
    [Parameter] public bool AbsoluteStroke { get; set; }

    /// <summary>CSS class forwarded to the &lt;svg&gt; element.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>Accessible title. When set, the icon is announced by screen readers via &lt;title&gt; + <c>role="img"</c> + <c>aria-labelledby</c>. When null, the icon is decorative and gets <c>aria-hidden="true"</c>.</summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Extra attributes forwarded to the root &lt;svg&gt; (e.g. <c>style</c>, <c>data-*</c>). Pass handlers
    /// without the <c>@</c> — <c>onclick="@(() => Handler())"</c> — or put them on a wrapping <c>&lt;button&gt;</c>.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [CascadingParameter] private IconDefaultValues? Defaults { get; set; }

    private string EffectiveSize => _sizeSet ? Size : Defaults?.Size ?? Size;

    private double EffectiveStrokeWidth => _strokeWidthSet ? StrokeWidth : Defaults?.StrokeWidth ?? StrokeWidth;

    private bool EffectiveAbsoluteStroke => _absoluteStrokeSet ? AbsoluteStroke : Defaults?.AbsoluteStroke ?? AbsoluteStroke;

    private string? EffectiveClass => IconDefaultValues.JoinClasses(Defaults?.Class, Class);

    /// <summary>Kebab-case catalog name for this icon (e.g. <c>"chevron-right"</c>). Used by the dispatcher lookup.</summary>
    protected abstract string IconName { get; }

    /// <summary>Emit the inner shape markup for the icon (one or more &lt;path&gt;/&lt;circle&gt;/etc. elements).</summary>
    protected abstract void EmitChildren(RenderTreeBuilder builder, int seq);

    /// <inheritdoc />
    public override Task SetParametersAsync(ParameterView parameters)
    {
        // Every render passes the icon's full parameter set, so absence means "not set here".
        _sizeSet = parameters.TryGetValue<string>(nameof(Size), out _);
        _strokeWidthSet = parameters.TryGetValue<double>(nameof(StrokeWidth), out _);
        _absoluteStrokeSet = parameters.TryGetValue<bool>(nameof(AbsoluteStroke), out _);
        return base.SetParametersAsync(parameters);
    }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var size = EffectiveSize;
        var strokeStr = ComputeStrokeWidth(size);
        var cssClass = EffectiveClass;

        builder.OpenElement(0, "svg");
        builder.AddAttribute(1, "xmlns", "http://www.w3.org/2000/svg");
        builder.AddAttribute(2, "width", size);
        builder.AddAttribute(3, "height", size);
        builder.AddAttribute(4, "viewBox", "0 0 24 24");
        builder.AddAttribute(5, "fill", "none");
        builder.AddAttribute(6, "stroke", "currentColor");
        builder.AddAttribute(7, "stroke-width", strokeStr);
        builder.AddAttribute(8, "stroke-linecap", "round");
        builder.AddAttribute(9, "stroke-linejoin", "round");

        if (cssClass is not null)
            builder.AddAttribute(10, "class", cssClass);

        if (Title is not null)
        {
            _titleId ??= $"shellicon-{Interlocked.Increment(ref _idCounter):x}";
            builder.AddAttribute(11, "role", "img");
            builder.AddAttribute(12, "aria-labelledby", _titleId);
        }
        else
        {
            builder.AddAttribute(13, "aria-hidden", "true");
        }

        if (AdditionalAttributes is not null)
        {
            ThrowOnEventDirectiveMisuse(AdditionalAttributes);
            builder.AddMultipleAttributes(14, AdditionalAttributes);
        }

        if (Title is not null)
        {
            builder.OpenElement(15, "title");
            builder.AddAttribute(16, "id", _titleId);
            builder.AddContent(17, Title);
            builder.CloseElement();
        }

        EmitChildren(builder, 18);

        builder.CloseElement();
    }

    /* On a component, Razor passes @onclick="Save" as a string attribute named "@onclick";
       the browser's setAttribute rejects that name and the render batch fails. */
    private void ThrowOnEventDirectiveMisuse(IReadOnlyDictionary<string, object> attributes)
    {
        foreach (var key in attributes.Keys)
        {
            if (key.StartsWith("@on", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"'{key}' on icon '{IconName}' has no effect: on a component Razor passes it as a plain string, " +
                    $"not an event handler. Put the handler on a wrapping <button {key}=\"…\"> (recommended for accessibility), " +
                    $"or pass it without the @: {key[1..]}=\"@(() => Handler())\".");
            }
        }
    }

    private string ComputeStrokeWidth(string size)
    {
        var strokeWidth = EffectiveStrokeWidth;
        if (!EffectiveAbsoluteStroke)
            return strokeWidth.ToString(CultureInfo.InvariantCulture);

        if (double.TryParse(size, NumberStyles.Float, CultureInfo.InvariantCulture, out var s) && s > 0)
        {
            var scaled = strokeWidth * 24d / s;
            return scaled.ToString("0.###", CultureInfo.InvariantCulture);
        }

        return strokeWidth.ToString(CultureInfo.InvariantCulture);
    }
}
