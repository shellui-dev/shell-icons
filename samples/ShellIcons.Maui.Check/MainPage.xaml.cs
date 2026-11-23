namespace ShellIcons.Maui.Check;

public partial class MainPage : ContentPage
{
    private int _taps;

    public MainPage()
    {
        InitializeComponent();
        Header.Text = $"ShellIcons.Maui on {DeviceInfo.Platform} {DeviceInfo.VersionString}, {DeviceDisplay.MainDisplayInfo.Density}x";
    }

    private void OnTapped(object? sender, TappedEventArgs e) => TapLabel.Text = $"Tapped {++_taps}";
}
