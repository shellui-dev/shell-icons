using Microsoft.AspNetCore.Components;

namespace ShellIcons;

// Shared body of the generated Icon.* factory methods, so each of them stays a one-liner.
internal static class IconFragment
{
    /* All-default calls return a cached fragment (no allocation per render); otherwise only
       non-default arguments are passed as parameters. */
    public static RenderFragment Create<T>(
        string size,
        double strokeWidth,
        bool absoluteStroke,
        string? cssClass,
        string? title,
        IReadOnlyDictionary<string, object>? additionalAttributes)
        where T : IconCore
    {
        if (size == "24" && strokeWidth == 2 && !absoluteStroke
            && cssClass is null && title is null && additionalAttributes is null)
        {
            return Cache<T>.Default;
        }

        return builder =>
        {
            builder.OpenComponent<T>(0);
            if (size != "24") builder.AddAttribute(1, nameof(IconCore.Size), size);
            if (strokeWidth != 2) builder.AddAttribute(2, nameof(IconCore.StrokeWidth), strokeWidth);
            if (absoluteStroke) builder.AddAttribute(3, nameof(IconCore.AbsoluteStroke), true);
            if (cssClass is not null) builder.AddAttribute(4, nameof(IconCore.Class), cssClass);
            if (title is not null) builder.AddAttribute(5, nameof(IconCore.Title), title);
            if (additionalAttributes is not null) builder.AddMultipleAttributes(6, additionalAttributes);
            builder.CloseComponent();
        };
    }

    // Generic holder: a cache entry exists only for icons actually used, so the trimmer can drop the rest.
    private static class Cache<T> where T : IconCore
    {
        public static readonly RenderFragment Default = builder =>
        {
            builder.OpenComponent<T>(0);
            builder.CloseComponent();
        };
    }
}
