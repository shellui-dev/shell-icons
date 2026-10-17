using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ShellIcons.Generator.Xaml;

internal sealed class SourceFile
{
    public SourceFile(string path, string content) { Path = path; Content = content; }
    public string Path { get; }
    public string Content { get; }
}

internal sealed class XamlIcon
{
    public XamlIcon(string kebab, string pascal, string pack, XamlIconShapes shapes, IconMetadata meta)
    {
        Kebab = kebab;
        Pascal = pascal;
        Pack = pack;
        Shapes = shapes;
        Meta = meta;
    }

    public string Kebab { get; }
    public string Pascal { get; }
    public string Pack { get; }
    public XamlIconShapes Shapes { get; }
    public IconMetadata Meta { get; }
}

// Reads the catalog's SVGs and JSON sidecars into one icon list shared by every XAML target.
internal static class XamlCatalog
{
    private static readonly DiagnosticDescriptor OverrideDescriptor = new(
        id: "SHELLICONS001",
        title: "Custom icon overrides Lucide icon",
        messageFormat: "Icon '{0}' from catalog/custom/ overrides the Lucide-derived icon of the same name",
        category: "ShellIcons",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedDescriptor = new(
        id: "SHELLICONS002",
        title: "Icon uses SVG the XAML targets can't draw",
        messageFormat: "Icon '{0}' ({1}) uses {2}; that part is skipped on the XAML targets. Stick to path/circle/ellipse/rect/line/polyline/polygon without transforms.",
        category: "ShellIcons",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ReservedDescriptor = new(
        id: "SHELLICONS004",
        title: "Icon name not representable",
        messageFormat: "Icon '{0}' maps to '{1}', which is reserved or not a valid identifier; it is skipped on the XAML targets",
        category: "ShellIcons",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OffCanvasDescriptor = new(
        id: "SHELLICONS005",
        title: "Shape outside the viewBox dropped",
        messageFormat: "Icon '{0}' ({1}) has shapes entirely outside the 24x24 viewBox; browsers clip them, so they are dropped on the XAML targets: {2}",
        category: "ShellIcons",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static XamlIcon[] Read(SourceProductionContext ctx, ImmutableArray<SourceFile> files)
    {
        var metadata = new Dictionary<string, IconMetadata>(StringComparer.Ordinal);
        foreach (var f in files.Where(f => f.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
            metadata[Key(f.Path)] = IconMetadataReader.Read(f.Content);

        var resolved = new Dictionary<string, XamlIcon>(StringComparer.Ordinal);
        foreach (var f in files.Where(f => f.Path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)))
        {
            var kebab = Path.GetFileNameWithoutExtension(f.Path);
            var pascal = Naming.KebabToPascal(kebab);
            if (pascal.Length == 0) continue;

            if (pascal == "None" || !char.IsLetter(pascal[0]))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(ReservedDescriptor, Location.None, kebab, pascal));
                continue;
            }

            var pack = ResolvePack(f.Path);
            var shapes = SvgShapeReader.Read(f.Content);

            if (shapes.Unsupported.Count > 0)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(UnsupportedDescriptor, Location.None,
                    kebab, pack, string.Join(", ", shapes.Unsupported.Distinct())));
            }

            if (shapes.OffCanvas.Count > 0)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(OffCanvasDescriptor, Location.None,
                    kebab, pack, string.Join("; ", shapes.OffCanvas)));
            }

            var icon = new XamlIcon(kebab, pascal, pack, shapes,
                metadata.TryGetValue(Key(f.Path), out var m) ? m : IconMetadata.Empty);

            if (!resolved.TryGetValue(kebab, out var existing))
            {
                resolved[kebab] = icon;
            }
            else if (pack == "custom" && existing.Pack != "custom")
            {
                resolved[kebab] = icon;
                ctx.ReportDiagnostic(Diagnostic.Create(OverrideDescriptor, Location.None, kebab));
            }
        }

        return resolved.Values.OrderBy(i => i.Kebab, StringComparer.Ordinal).ToArray();
    }

    // Pairs "…/catalog/lucide/icons/house.svg" with "…/catalog/lucide/icons/house.json".
    private static string Key(string path)
    {
        var normalized = path.Replace('\\', '/');
        var dot = normalized.LastIndexOf('.');
        return dot < 0 ? normalized : normalized.Substring(0, dot);
    }

    private static string ResolvePack(string path) =>
        path.Replace('\\', '/').Contains("/catalog/custom/") ? "custom" : "lucide";
}
