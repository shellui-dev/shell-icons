namespace ShellIcons.Maui;

/// <summary>
/// Renders any icon by <see cref="IconName"/>, so an icon can be a property value or binding.
/// Keeps the whole catalog; typed controls (<c>&lt;icons:Search /&gt;</c>) trim better.
/// </summary>
public class Icon : IconView
{
    /// <summary>The icon to show. <see cref="IconName.None"/> renders nothing.</summary>
    public static readonly BindableProperty NameProperty = BindableProperty.Create(
        nameof(Name), typeof(IconName), typeof(Icon), IconName.None,
        propertyChanged: (b, _, n) => ((Icon)b).Load((IconName)n));

    /// <inheritdoc cref="NameProperty"/>
    public IconName Name
    {
        get => (IconName)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    private void Load(IconName name)
    {
        // A former name (Trash2) shows the renamed icon and reports its current name.
        var current = IconData.Resolve(name);
        var (stroke, fill) = IconData.Get(current);
        SetData(current, stroke, fill);
    }
}
