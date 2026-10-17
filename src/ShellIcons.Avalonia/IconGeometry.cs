using System.Collections.Concurrent;
using Avalonia.Media;

namespace ShellIcons.Avalonia;

/* The generator's path data is already plain "M L C Q A Z", which Avalonia parses natively.
   Geometry is cached and shared by every control showing the same icon. */
internal static class IconGeometry
{
    private static readonly ConcurrentDictionary<string, Geometry> Cache = new(StringComparer.Ordinal);

    public static Geometry? Get(string? data) =>
        string.IsNullOrEmpty(data) ? null : Cache.GetOrAdd(data, static d => StreamGeometry.Parse(d));
}
