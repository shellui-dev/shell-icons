namespace ShellIcons.Maui.Check;

public class App : Application
{
    public App()
    {
        // Icons on the page take their color from these resources, so a theme change recolors them.
        ApplyTheme(RequestedTheme);
        RequestedThemeChanged += (_, e) => ApplyTheme(e.RequestedTheme);
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(new MainPage());

    private void ApplyTheme(AppTheme theme)
    {
        var dark = theme == AppTheme.Dark;
        Resources["IconColor"] = dark ? Colors.White : Colors.Black;
        Resources["PageColor"] = dark ? Colors.Black : Colors.White;
        Resources["TextColor"] = dark ? Colors.White : Colors.Black;
    }
}
