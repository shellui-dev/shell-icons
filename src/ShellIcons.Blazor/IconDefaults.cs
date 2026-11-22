using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ShellIcons;

/// <summary>
/// Sets <c>Size</c>, <c>StrokeWidth</c>, <c>AbsoluteStroke</c> and a <c>Class</c> for every icon inside it, so a
/// toolbar or menu sizes its icons once. A value set on an icon wins; nested defaults override outer ones,
/// and classes from every level are combined.
/// <code>
/// &lt;IconDefaults Size="16" StrokeWidth="1.5"&gt;
///     &lt;SaveIcon /&gt; &lt;CopyIcon /&gt; &lt;TrashIcon Size="20" /&gt;
/// &lt;/IconDefaults&gt;
/// </code>
/// </summary>
public sealed class IconDefaults : ComponentBase
{
    [CascadingParameter] private IconDefaultValues? Parent { get; set; }

    /// <summary>Default <c>Size</c> for icons inside, e.g. <c>"16"</c> or <c>"1.25em"</c>.</summary>
    [Parameter] public string? Size { get; set; }

    /// <summary>Default <c>StrokeWidth</c> for icons inside.</summary>
    [Parameter] public double? StrokeWidth { get; set; }

    /// <summary>Default <c>AbsoluteStroke</c> for icons inside.</summary>
    [Parameter] public bool? AbsoluteStroke { get; set; }

    /// <summary>CSS class added to every icon inside, before the icon's own <c>Class</c>.</summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>The content whose icons take these defaults.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var values = new IconDefaultValues(
            Size ?? Parent?.Size,
            StrokeWidth ?? Parent?.StrokeWidth,
            AbsoluteStroke ?? Parent?.AbsoluteStroke,
            IconDefaultValues.JoinClasses(Parent?.Class, Class));

        builder.OpenComponent<CascadingValue<IconDefaultValues>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<IconDefaultValues>.Value), values);
        builder.AddAttribute(2, nameof(CascadingValue<IconDefaultValues>.ChildContent), ChildContent);
        builder.CloseComponent();
    }
}

// The merged defaults an IconDefaults cascades to the icons inside it.
internal sealed record IconDefaultValues(string? Size, double? StrokeWidth, bool? AbsoluteStroke, string? Class)
{
    public static string? JoinClasses(string? first, string? second)
    {
        if (string.IsNullOrWhiteSpace(first)) return string.IsNullOrWhiteSpace(second) ? null : second;
        if (string.IsNullOrWhiteSpace(second)) return first;
        return first + " " + second;
    }
}
