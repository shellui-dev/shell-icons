using Avalonia;

namespace ShellIcons.Avalonia;

/// <summary>
/// Renders any icon by <see cref="IconName"/>, so an icon can be a property value or binding.
/// Keeps the whole catalog; typed controls (<c>&lt;icons:Search /&gt;</c>) trim better.
/// </summary>
public class Icon : IconView
{
    /// <summary>The icon to show. <see cref="IconName.None"/> renders nothing.</summary>
    /// <remarks>Called <c>Kind</c> because Avalonia's <c>Name</c> is already the element name.</remarks>
    public static readonly StyledProperty<IconName> KindProperty =
        AvaloniaProperty.Register<Icon, IconName>(nameof(Kind));

    static Icon()
    {
        KindProperty.Changed.AddClassHandler<Icon>((icon, e) => icon.Load(e.GetNewValue<IconName>()));
    }

    /// <inheritdoc cref="KindProperty"/>
    public IconName Kind
    {
        get => GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    private void Load(IconName kind)
    {
        // A former name (Trash2) shows the renamed icon and reports its current name.
        var current = IconData.Resolve(kind);
        var (stroke, fill) = IconData.Get(current);
        SetData(current, stroke, fill);
    }
}
