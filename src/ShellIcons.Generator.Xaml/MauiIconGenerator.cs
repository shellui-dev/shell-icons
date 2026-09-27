using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ShellIcons.Generator.Xaml;

/* From the same catalog as the Blazor generator, emits: the IconName enum, IconData (path data for
   the Icon dispatcher), IconCatalog metadata, and one typed control per icon carrying its own path. */
[Generator]
public sealed class MauiIconGenerator : IIncrementalGenerator
{
    private const string Ns = "ShellIcons.Maui";
    private const string IconsNs = "ShellIcons.Maui.Icons";

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
        messageFormat: "Icon '{0}' ({1}) uses {2}; that part is skipped in ShellIcons.Maui. Stick to path/circle/ellipse/rect/line/polyline/polygon without transforms.",
        category: "ShellIcons",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor OffCanvasDescriptor = new(
        id: "SHELLICONS005",
        title: "Shape outside the viewBox dropped",
        messageFormat: "Icon '{0}' ({1}) has shapes entirely outside the 24x24 viewBox; browsers clip them, so they are dropped for ShellIcons.Maui: {2}",
        category: "ShellIcons",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ReservedDescriptor = new(
        id: "SHELLICONS004",
        title: "Icon name not representable",
        messageFormat: "Icon '{0}' maps to '{1}', which is reserved or not a valid identifier; it is skipped in ShellIcons.Maui",
        category: "ShellIcons",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var files = context.AdditionalTextsProvider
            .Where(t => t.Path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                     || t.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .Select((t, ct) => new SourceFile(t.Path, t.GetText(ct)?.ToString() ?? string.Empty))
            .Collect();

        context.RegisterSourceOutput(files, Emit);
    }

    private sealed class SourceFile
    {
        public SourceFile(string path, string content) { Path = path; Content = content; }
        public string Path { get; }
        public string Content { get; }
    }

    private sealed class MauiIcon
    {
        public string Kebab = "";
        public string Pascal = "";
        public string Pack = "";
        public XamlIconShapes Shapes = null!;
        public IconMetadata Meta = IconMetadata.Empty;
    }

    private static void Emit(SourceProductionContext ctx, ImmutableArray<SourceFile> files)
    {
        var metadata = new Dictionary<string, IconMetadata>(StringComparer.Ordinal);
        foreach (var f in files.Where(f => f.Path.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
            metadata[Key(f.Path)] = IconMetadataReader.Read(f.Content);

        var resolved = new Dictionary<string, MauiIcon>(StringComparer.Ordinal);
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

            var icon = new MauiIcon
            {
                Kebab = kebab,
                Pascal = pascal,
                Pack = pack,
                Shapes = shapes,
                Meta = metadata.TryGetValue(Key(f.Path), out var m) ? m : IconMetadata.Empty,
            };

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

        var icons = resolved.Values.OrderBy(i => i.Kebab, StringComparer.Ordinal).ToArray();

        ctx.AddSource("IconName.g.cs", SourceText.From(EmitEnum(icons), Encoding.UTF8));
        ctx.AddSource("IconData.g.cs", SourceText.From(EmitData(icons), Encoding.UTF8));
        ctx.AddSource("IconCatalog.g.cs", SourceText.From(EmitCatalog(icons), Encoding.UTF8));
        foreach (var icon in icons)
            ctx.AddSource($"Icons/{icon.Pascal}.g.cs", SourceText.From(EmitTypedControl(icon), Encoding.UTF8));
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

    private static string EmitEnum(MauiIcon[] icons)
    {
        var sb = Header();
        sb.AppendLine($"namespace {Ns};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>Every icon in the ShellIcons catalog. PascalCase of the Lucide file name (<c>chevron-right</c> → <c>ChevronRight</c>).</summary>");
        sb.AppendLine("public enum IconName");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>No icon. The <c>Icon</c> control renders nothing.</summary>");
        sb.AppendLine("    None = 0,");
        for (var i = 0; i < icons.Length; i++)
        {
            sb.AppendLine($"    /// <summary><c>{icons[i].Kebab}</c> ({icons[i].Pack}).</summary>");
            sb.AppendLine($"    {icons[i].Pascal} = {i + 1},");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string EmitData(MauiIcon[] icons)
    {
        var sb = Header();
        sb.AppendLine($"namespace {Ns};");
        sb.AppendLine();
        sb.AppendLine("internal static class IconData");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>Stroke and (optional) fill path data for <paramref name=\"name\"/>. Roots the full catalog — only the <c>Icon</c> dispatcher uses it.</summary>");
        sb.AppendLine("    public static (string Stroke, string? Fill) Get(IconName name) => name switch");
        sb.AppendLine("    {");
        foreach (var icon in icons)
            sb.AppendLine($"        IconName.{icon.Pascal} => ({Literal(icon.Shapes.StrokeData)}, {LiteralOrNull(icon.Shapes.FillData)}),");
        sb.AppendLine("        _ => (string.Empty, null),");
        sb.AppendLine("    };");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string EmitCatalog(MauiIcon[] icons)
    {
        var sb = Header();
        sb.AppendLine($"namespace {Ns};");
        sb.AppendLine();
        sb.AppendLine("public static partial class IconCatalog");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>Number of icons in the catalog (excluding <see cref=\"IconName.None\"/>).</summary>");
        sb.AppendLine($"    public const int Count = {icons.Length};");
        sb.AppendLine();
        sb.AppendLine("    private static IconInfo? Create(IconName name) => name switch");
        sb.AppendLine("    {");
        foreach (var icon in icons)
        {
            sb.Append($"        IconName.{icon.Pascal} => new IconInfo(IconName.{icon.Pascal}, {Literal(icon.Kebab)}, {Literal(icon.Pack)}, ");
            sb.Append(Array(icon.Meta.Tags)).Append(", ").Append(Array(icon.Meta.Categories)).Append(", ").Append(Array(icon.Meta.Aliases));
            sb.AppendLine("),");
        }
        sb.AppendLine("        _ => null,");
        sb.AppendLine("    };");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string EmitTypedControl(MauiIcon icon)
    {
        var sb = Header();
        sb.AppendLine($"namespace {IconsNs};");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>Icon <c>{icon.Kebab}</c> ({icon.Pack}). Carries its own path data, so unused icons are trimmed.</summary>");
        sb.AppendLine($"public sealed class {icon.Pascal} : global::{Ns}.IconView");
        sb.AppendLine("{");
        sb.AppendLine($"    /// <summary>Creates the <c>{icon.Kebab}</c> icon.</summary>");
        sb.AppendLine($"    public {icon.Pascal}() : base(global::{Ns}.IconName.{icon.Pascal}, {Literal(icon.Shapes.StrokeData)}, {LiteralOrNull(icon.Shapes.FillData)}) {{ }}");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static StringBuilder Header()
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        return sb;
    }

    private static string Array(IReadOnlyList<string> values) =>
        values.Count == 0
            ? "global::System.Array.Empty<string>()"
            : "new[] { " + string.Join(", ", values.Select(Literal)) + " }";

    private static string LiteralOrNull(string? value) => value is null ? "null" : Literal(value);

    private static string Literal(string value)
    {
        var sb = new StringBuilder(value.Length + 2);
        sb.Append('"');
        foreach (var c in value)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < ' ') sb.Append("\\u").Append(((int)c).ToString("x4"));
                    else sb.Append(c);
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }
}
