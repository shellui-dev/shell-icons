using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ShellIcons.Avalonia.Tests;

// Renders through Skia and reads pixels back, so a geometry Avalonia can't parse or draws off-grid fails here.
public class RenderTests
{
    private const int Cell = 24;

    [AvaloniaFact]
    public void EveryIcon_ParsesAndStaysOnTheGrid()
    {
        var failures = new List<string>();

        foreach (var name in Enum.GetValues<IconName>().Where(n => n != IconName.None))
        {
            var (stroke, fill) = IconData.Get(name);
            if (string.IsNullOrEmpty(stroke) && fill is null)
            {
                failures.Add($"{name}: no shapes");
                continue;
            }

            foreach (var data in new[] { stroke, fill }.Where(d => !string.IsNullOrEmpty(d)))
            {
                try
                {
                    // Bounds follow the curve itself, so control points poking out don't count.
                    var b = IconGeometry.Get(data)!.Bounds;
                    if (b.Left < -0.5 || b.Top < -0.5 || b.Right > 24.5 || b.Bottom > 24.5)
                        failures.Add($"{name}: bounds {b} are off the 24x24 grid");
                }
                catch (Exception ex)
                {
                    failures.Add($"{name}: {ex.Message}");
                }
            }
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures.Take(25)));
    }

    [AvaloniaFact]
    public void EveryIcon_DrawsInk_InsideItsCell()
    {
        var names = Enum.GetValues<IconName>().Where(n => n != IconName.None).ToArray();
        const int columns = 40;
        var rows = (names.Length + columns - 1) / columns;

        var grid = new UniformGrid { Columns = columns, Rows = rows };
        foreach (var name in names)
            grid.Children.Add(new Icon { Kind = name, Foreground = Brushes.Black });

        using var frame = Capture(grid, columns * Cell, rows * Cell);
        var pixels = Pixels(frame);

        var blank = new List<IconName>();
        for (var i = 0; i < names.Length; i++)
        {
            if (CountInk(pixels, frame.PixelSize.Width, i % columns * Cell, i / columns * Cell, Cell, Cell) == 0)
                blank.Add(names[i]);
        }

        Save(frame, "catalog.png");
        Assert.True(blank.Count == 0, "Icons that drew nothing: " + string.Join(", ", blank.Take(25)));
    }

    [AvaloniaFact]
    public void Size_ScalesTheDrawing()
    {
        var small = InkBounds(new Icons.Square { Size = 24, Foreground = Brushes.Black }, 96);
        var large = InkBounds(new Icons.Square { Size = 72, Foreground = Brushes.Black }, 96);

        // Lucide's square spans 3..21 with a 2-unit stroke: 20 of the 24 grid units.
        Assert.InRange(small.Width, 19, 21);
        Assert.InRange(large.Width, 58, 62);
    }

    [AvaloniaFact]
    public void AbsoluteStroke_DrawsAThinnerLineThanTheScaledDefault()
    {
        var scaled = Ink(new Icons.Minus { Size = 72, Foreground = Brushes.Black }, 96);
        var absolute = Ink(new Icons.Minus { Size = 72, AbsoluteStroke = true, Foreground = Brushes.Black }, 96);

        // Scaled stroke is 6px, absolute stays 2px: roughly a third of the ink.
        Assert.InRange((double)absolute / scaled, 0.2, 0.5);
    }

    [AvaloniaFact]
    public void Foreground_ColorsTheStroke()
    {
        using var frame = Capture(new Icons.Square { Size = 48, Foreground = Brushes.Red }, 48, 48);
        var pixels = Pixels(frame);

        // Middle of the left edge (x = 3 grid units → 6px).
        var offset = (24 * frame.PixelSize.Width + 6) * 4;
        Assert.True(pixels[offset + 2] > 200, "red channel");
        Assert.True(pixels[offset + 1] < 60, "green channel");
        Assert.True(pixels[offset] < 60, "blue channel");
    }

    [AvaloniaFact]
    public void ContactSheet_ForEyeballing()
    {
        IconName[] names =
        [
            IconName.House, IconName.Search, IconName.Settings, IconName.CircleCheck, IconName.Heart,
            IconName.ShoppingCart, IconName.Zap, IconName.Github, IconName.Twitter, IconName.SaveOff,
        ];

        var panel = new StackPanel { Spacing = 12, Margin = new global::Avalonia.Thickness(12) };
        foreach (var size in new[] { 16d, 24, 48 })
        {
            var row = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal, Spacing = 12 };
            foreach (var name in names)
                row.Children.Add(new Icon { Kind = name, Size = size, Foreground = Brushes.Black });
            panel.Children.Add(row);
        }

        var filled = new StackPanel { Orientation = global::Avalonia.Layout.Orientation.Horizontal, Spacing = 12 };
        foreach (var name in Enum.GetValues<IconName>().Where(n => IconData.Get(n).Fill is not null).Take(10))
            filled.Children.Add(new Icon { Kind = name, Size = 48, Foreground = Brushes.RoyalBlue });
        panel.Children.Add(filled);

        using var frame = Capture(panel, 640, 240);
        Save(frame, "contact-sheet.png");
    }

    private static WriteableBitmap Capture(Control content, int width, int height)
    {
        var window = new Window
        {
            Width = width,
            Height = height,
            Background = Brushes.White,
            Content = content,
        };
        window.Show();
        var frame = window.CaptureRenderedFrame() ?? throw new InvalidOperationException("No frame rendered.");
        window.Close();
        return frame;
    }

    private static byte[] Pixels(WriteableBitmap bitmap)
    {
        using var buffer = bitmap.Lock();
        Assert.True(buffer.Format == PixelFormat.Bgra8888 || buffer.Format == PixelFormat.Rgba8888, buffer.Format.ToString());

        var pixels = new byte[buffer.Size.Width * buffer.Size.Height * 4];
        for (var y = 0; y < buffer.Size.Height; y++)
        {
            System.Runtime.InteropServices.Marshal.Copy(
                buffer.Address + y * buffer.RowBytes, pixels, y * buffer.Size.Width * 4, buffer.Size.Width * 4);
        }

        // Normalize to BGRA so callers can index channels the same way.
        if (buffer.Format == PixelFormat.Rgba8888)
        {
            for (var i = 0; i < pixels.Length; i += 4)
                (pixels[i], pixels[i + 2]) = (pixels[i + 2], pixels[i]);
        }
        return pixels;
    }

    private static bool IsInk(byte[] pixels, int offset) =>
        pixels[offset] < 128 || pixels[offset + 1] < 128 || pixels[offset + 2] < 128;

    private static int CountInk(byte[] pixels, int stride, int x0, int y0, int width, int height)
    {
        var count = 0;
        for (var y = y0; y < y0 + height; y++)
            for (var x = x0; x < x0 + width; x++)
                if (IsInk(pixels, (y * stride + x) * 4)) count++;
        return count;
    }

    private static int Ink(Control icon, int canvas)
    {
        using var frame = Capture(icon, canvas, canvas);
        return CountInk(Pixels(frame), frame.PixelSize.Width, 0, 0, frame.PixelSize.Width, frame.PixelSize.Height);
    }

    private static (int Width, int Height) InkBounds(Control icon, int canvas)
    {
        using var frame = Capture(icon, canvas, canvas);
        var pixels = Pixels(frame);
        int w = frame.PixelSize.Width, h = frame.PixelSize.Height;
        int minX = w, minY = h, maxX = -1, maxY = -1;

        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                if (!IsInk(pixels, (y * w + x) * 4)) continue;
                minX = Math.Min(minX, x); maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y); maxY = Math.Max(maxY, y);
            }

        return (maxX - minX + 1, maxY - minY + 1);
    }

    private static void Save(WriteableBitmap frame, string fileName)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "renders");
        Directory.CreateDirectory(dir);
        frame.Save(Path.Combine(dir, fileName), PngBitmapEncoderOptions.Default);
    }
}
