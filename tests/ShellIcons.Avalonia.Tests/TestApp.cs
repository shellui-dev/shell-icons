using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using ShellIcons.Avalonia.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace ShellIcons.Avalonia.Tests;

public class TestApp : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());
}

public static class TestAppBuilder
{
    // UseHeadlessDrawing = false swaps the no-op renderer for Skia, so captured frames have pixels.
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
