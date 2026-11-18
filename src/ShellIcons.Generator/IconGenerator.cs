using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using ShellIcons.Generator.Xaml;

namespace ShellIcons.Generator;

/* Emits typed components, their {Name}Icon aliases, the ShellIcon dispatcher, the Icon factory and
   IconCatalog from catalog/lucide and catalog/custom SVGs. A custom icon overrides a Lucide one of the
   same name. Names the JSON sidecars list as aliases (an icon's former names) render that icon too. */
[Generator]
public sealed class IconGenerator : IIncrementalGenerator
{
    private const string DispatcherNamespace = "ShellIcons";
    private const string IconsNamespace = "ShellIcons.Icons";

    private static readonly DiagnosticDescriptor OverrideDescriptor = new(
        id: "SHELLICONS001",
        title: "Custom icon overrides Lucide icon",
        messageFormat: "Icon '{0}' from catalog/custom/ overrides the Lucide-derived icon of the same name",
        category: "ShellIcons",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor SuffixReservedDescriptor = new(
        id: "SHELLICONS003",
        title: "Suffixed icon component skipped",
        messageFormat: "Icon '{0}' has no <{1} /> suffixed component because '{1}' is a reserved ShellIcons type; use Icon.{2}() or <ShellIcons.Icons.{2} /> instead",
        category: "ShellIcons",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    // Existing root-namespace types a {Name}Icon alias would clash with (Lucide's `shell` → ShellIcon).
    private static readonly HashSet<string> ReservedRootNames = new(System.StringComparer.Ordinal)
    {
        "ShellIcon", "Icon", "IconCore", "IconFragment",
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var svgs = context.AdditionalTextsProvider
            .Where(t => t.Path.EndsWith(".svg", System.StringComparison.OrdinalIgnoreCase));

        var definitions = svgs
            .Select((text, ct) =>
            {
                var content = text.GetText(ct)?.ToString() ?? string.Empty;
                var kebab = Path.GetFileNameWithoutExtension(text.Path);
                var pascal = Naming.KebabToPascal(kebab);
                var inner = SvgParser.ExtractInner(content);
                var pack = ResolvePack(text.Path);
                return new IconDefinition(kebab, pascal, inner, pack);
            })
            .Where(d => !string.IsNullOrEmpty(d.KebabName) && !string.IsNullOrEmpty(d.InnerMarkup))
            .Collect();

        // Icon sidecars and Lucide's tag groups: aliases, tags and categories.
        var sidecars = context.AdditionalTextsProvider
            .Where(t => t.Path.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
            .Select((text, ct) => new KeyValuePair<string, string>(text.Path, text.GetText(ct)?.ToString() ?? string.Empty))
            .Collect();

        context.RegisterSourceOutput(definitions.Combine(sidecars), (ctx, input) => EmitAll(ctx, input.Left, input.Right));
    }

    private static string ResolvePack(string path)
    {
        var normalized = path.Replace('\\', '/');
        if (normalized.Contains("/catalog/custom/")) return "custom";
        if (normalized.Contains("/catalog/lucide/")) return "lucide";
        return "lucide";
    }

    private static void EmitAll(SourceProductionContext ctx, ImmutableArray<IconDefinition> defs, ImmutableArray<KeyValuePair<string, string>> sidecars)
    {
        var resolved = new Dictionary<string, IconDefinition>();

        foreach (var d in defs)
        {
            if (!resolved.TryGetValue(d.KebabName, out var existing))
            {
                resolved[d.KebabName] = d;
                continue;
            }

            if (d.SourcePack == "custom" && existing.SourcePack != "custom")
            {
                resolved[d.KebabName] = d;
                ctx.ReportDiagnostic(Diagnostic.Create(OverrideDescriptor, Location.None, d.KebabName));
            }
        }

        var ordered = resolved.Values.OrderBy(d => d.KebabName, System.StringComparer.Ordinal).ToArray();

        var metaById = IconMetadataReader.ReadCatalog(sidecars);
        var aliases = IconAliases.Resolve(ordered
            .Select(d => metaById.TryGetValue(d.KebabName, out var m)
                ? new AliasSource(d.KebabName, d.PascalName, m.Aliases, m.DeprecatedAliases)
                : new AliasSource(d.KebabName, d.PascalName, new string[0], new string[0])));

        // Hint names use the kebab id: they're compared ignoring case, and letter-s and letters differ only by case.
        foreach (var d in ordered)
            ctx.AddSource($"Icons/{d.KebabName}.g.cs", SourceText.From(EmitTypedComponent(d), Encoding.UTF8));

        ctx.AddSource("IconAliases.g.cs", SourceText.From(EmitAliasComponents(aliases), Encoding.UTF8));
        ctx.AddSource("SuffixedIcons.g.cs", SourceText.From(EmitSuffixedComponents(ctx, ordered, aliases), Encoding.UTF8));
        ctx.AddSource("ShellIcon.g.cs", SourceText.From(EmitDispatcher(ordered, DispatcherAliases(ordered, metaById)), Encoding.UTF8));
        ctx.AddSource("Icon.g.cs", SourceText.From(EmitFactoryDispatcher(ordered, aliases), Encoding.UTF8));
        ctx.AddSource("IconCatalog.g.cs", SourceText.From(BlazorCatalogEmitter.Emit(ordered, metaById, aliases), Encoding.UTF8));
    }

    // Every alias id the dispatcher accepts, including spelling variants that get no C# name of their own.
    private static List<KeyValuePair<string, string>> DispatcherAliases(IconDefinition[] defs, Dictionary<string, IconMetadata> metaById)
    {
        var ids = new HashSet<string>(defs.Select(d => d.KebabName), System.StringComparer.Ordinal);
        var result = new List<KeyValuePair<string, string>>();
        foreach (var d in defs)
        {
            if (!metaById.TryGetValue(d.KebabName, out var meta)) continue;
            foreach (var alias in meta.Aliases)
            {
                if (ids.Add(alias)) result.Add(new KeyValuePair<string, string>(alias, d.KebabName));
            }
        }
        return result;
    }

    private static string ObsoleteAttribute(IconAlias alias, string replacement) =>
        alias.Deprecated
            ? $"[global::System.Obsolete(\"{IconAliases.ObsoleteMessage(alias, replacement)}\")]\n"
            : string.Empty;

    private static string EmitAliasComponents(IReadOnlyList<IconAlias> aliases)
    {
        // Subclasses of the renamed icon, so markup written against an older Lucide keeps compiling.
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {IconsNamespace};");

        foreach (var a in aliases)
        {
            sb.AppendLine();
            sb.AppendLine($"/// <summary>Former name of <see cref=\"{a.TargetPascal}\"/> (<c>{a.Kebab}</c> → <c>{a.TargetKebab}</c>).</summary>");
            sb.Append(ObsoleteAttribute(a, a.TargetPascal));
            sb.AppendLine($"public sealed class {a.Pascal} : {a.TargetPascal} {{ }}");
        }

        return sb.ToString();
    }

    private static string EmitTypedComponent(IconDefinition d)
    {
        var innerLiteral = ToVerbatimStringLiteral(d.InnerMarkup);
        return $$"""
            // <auto-generated/>
            #nullable enable
            using Microsoft.AspNetCore.Components.Rendering;

            namespace {{IconsNamespace}};

            /// <summary>Icon <c>{{d.KebabName}}</c> (source: {{d.SourcePack}}).</summary>
            public class {{d.PascalName}} : global::ShellIcons.IconCore
            {
                /// <inheritdoc />
                protected override string IconName => "{{d.KebabName}}";

                /// <inheritdoc />
                protected override void EmitChildren(RenderTreeBuilder builder, int seq) =>
                    builder.AddMarkupContent(seq, {{innerLiteral}});
            }
            """;
    }

    private static string EmitSuffixedComponents(SourceProductionContext ctx, IconDefinition[] defs, IReadOnlyList<IconAlias> aliases)
    {
        /* The suffix keeps names clear of UI-kit components (RZ9985) under a single @using ShellIcons.
           Empty subclasses, so they cost metadata only. */
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {DispatcherNamespace};");

        foreach (var d in defs)
        {
            var suffixed = d.PascalName + "Icon";
            if (ReservedRootNames.Contains(suffixed))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(SuffixReservedDescriptor, Location.None, d.KebabName, suffixed, d.PascalName));
                continue;
            }

            sb.AppendLine();
            sb.AppendLine($"/// <summary>Icon <c>{d.KebabName}</c> (source: {d.SourcePack}). Suffixed alias of <see cref=\"global::{IconsNamespace}.{d.PascalName}\"/>.</summary>");
            sb.AppendLine($"public sealed class {suffixed} : global::{IconsNamespace}.{d.PascalName} {{ }}");
        }

        foreach (var a in aliases)
        {
            var suffixed = a.Pascal + "Icon";
            var target = a.TargetPascal + "Icon";
            if (ReservedRootNames.Contains(suffixed) || ReservedRootNames.Contains(target)) continue;

            sb.AppendLine();
            sb.AppendLine($"/// <summary>Former name of <see cref=\"{target}\"/> (<c>{a.Kebab}</c> → <c>{a.TargetKebab}</c>).</summary>");
            sb.Append(ObsoleteAttribute(a, target));
            sb.AppendLine($"public sealed class {suffixed} : global::{IconsNamespace}.{a.TargetPascal} {{ }}");
        }

        return sb.ToString();
    }

    private static string EmitDispatcher(IconDefinition[] defs, List<KeyValuePair<string, string>> aliases)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Microsoft.AspNetCore.Components;");
        sb.AppendLine("using Microsoft.AspNetCore.Components.Rendering;");
        sb.AppendLine();
        sb.AppendLine($"namespace {DispatcherNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>Renders any icon by kebab-case name, e.g. <c>&lt;ShellIcon Name=\"chevron-right\" /&gt;</c>.");
        sb.AppendLine("/// Referencing it keeps the whole catalog; prefer the typed components when the name is known.</summary>");
        sb.AppendLine("public sealed class ShellIcon : IconCore");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>Kebab-case name of the icon to render (e.g. <c>\"chevron-right\"</c>). Former Lucide names work too.</summary>");
        sb.AppendLine("    [Parameter] public string Name { get; set; } = string.Empty;");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine("    protected override string IconName => Name;");
        sb.AppendLine();
        sb.AppendLine("    private static readonly Dictionary<string, string> _catalog = new(StringComparer.Ordinal)");
        sb.AppendLine("    {");
        foreach (var d in defs)
        {
            var lit = ToVerbatimStringLiteral(d.InnerMarkup);
            sb.AppendLine($"        [\"{d.KebabName}\"] = {lit},");
        }
        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    // Former names, e.g. \"trash-2\" → \"trash\".");
        sb.AppendLine("    private static readonly Dictionary<string, string> _aliases = new(StringComparer.Ordinal)");
        sb.AppendLine("    {");
        foreach (var a in aliases)
            sb.AppendLine($"        [\"{a.Key}\"] = \"{a.Value}\",");
        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine("    protected override void EmitChildren(RenderTreeBuilder builder, int seq)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (Name is null) return;");
        sb.AppendLine("        if (_catalog.TryGetValue(Name, out var markup)");
        sb.AppendLine("            || (_aliases.TryGetValue(Name, out var current) && _catalog.TryGetValue(current, out markup)))");
        sb.AppendLine("            builder.AddMarkupContent(seq, markup);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>Every icon name in the catalog, in sorted order.</summary>");
        sb.AppendLine("    public static IReadOnlyCollection<string> Names => _catalog.Keys;");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string EmitFactoryDispatcher(IconDefinition[] defs, IReadOnlyList<IconAlias> aliases)
    {
        /* `Icon`, not `Icons`: the plural is taken by the ShellIcons.Icons namespace.
           Each method references only its own component, so unused icons still trim. */
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Microsoft.AspNetCore.Components;");
        sb.AppendLine();
        sb.AppendLine($"namespace {DispatcherNamespace};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Every icon as a <see cref=\"RenderFragment\"/> — for icons as values (component parameters, lists).");
        sb.AppendLine("/// <code>");
        sb.AppendLine("/// @using ShellIcons");
        sb.AppendLine("/// @Icon.Plus()");
        sb.AppendLine("/// @Icon.Bell(size: \"16\", strokeWidth: 1.5, cssClass: \"text-primary\")");
        sb.AppendLine("/// </code>");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class Icon");
        sb.AppendLine("{");

        for (var i = 0; i < defs.Length; i++)
        {
            var d = defs[i];
            var typeRef = $"global::{IconsNamespace}.{d.PascalName}";
            sb.AppendLine();
            sb.AppendLine($"    /// <summary>Renders the <c>{d.KebabName}</c> icon (source: {d.SourcePack}).</summary>");
            sb.AppendLine($"    public static RenderFragment {d.PascalName}(string size = \"24\", double strokeWidth = 2, bool absoluteStroke = false, string? cssClass = null, string? title = null, global::System.Collections.Generic.IReadOnlyDictionary<string, object>? additionalAttributes = null) =>");
            sb.AppendLine($"        global::ShellIcons.IconFragment.Create<{typeRef}>(size, strokeWidth, absoluteStroke, cssClass, title, additionalAttributes);");
        }

        foreach (var a in aliases)
        {
            var typeRef = $"global::{IconsNamespace}.{a.TargetPascal}";
            sb.AppendLine();
            sb.AppendLine($"    /// <summary>Former name of <see cref=\"{a.TargetPascal}\"/> (<c>{a.Kebab}</c> → <c>{a.TargetKebab}</c>).</summary>");
            if (a.Deprecated) sb.Append("    ").Append(ObsoleteAttribute(a, a.TargetPascal + "()"));
            sb.AppendLine($"    public static RenderFragment {a.Pascal}(string size = \"24\", double strokeWidth = 2, bool absoluteStroke = false, string? cssClass = null, string? title = null, global::System.Collections.Generic.IReadOnlyDictionary<string, object>? additionalAttributes = null) =>");
            sb.AppendLine($"        global::ShellIcons.IconFragment.Create<{typeRef}>(size, strokeWidth, absoluteStroke, cssClass, title, additionalAttributes);");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string ToVerbatimStringLiteral(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return $"@\"{escaped}\"";
    }
}
