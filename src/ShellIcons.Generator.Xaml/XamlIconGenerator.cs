using System;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ShellIcons.Generator.Xaml;

/* Generates a XAML target's catalog. The consuming project picks the target with
   <ShellIconsTarget>Maui|Avalonia</ShellIconsTarget> (made visible via CompilerVisibleProperty). */
[Generator]
public sealed class XamlIconGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor UnknownTargetDescriptor = new(
        id: "SHELLICONS006",
        title: "ShellIconsTarget not set",
        messageFormat: "ShellIcons.Generator.Xaml needs <ShellIconsTarget> set to 'Maui' or 'Avalonia' (got '{0}'); no icons were generated",
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

        var target = context.AnalyzerConfigOptionsProvider.Select((options, _) =>
            options.GlobalOptions.TryGetValue("build_property.ShellIconsTarget", out var value) ? value.Trim() : string.Empty);

        context.RegisterSourceOutput(files.Combine(target), (ctx, input) => Emit(ctx, input.Left, input.Right));
    }

    private static void Emit(SourceProductionContext ctx, ImmutableArray<SourceFile> files, string target)
    {
        var ns = NamespaceFor(target);
        if (ns is null)
        {
            ctx.ReportDiagnostic(Diagnostic.Create(UnknownTargetDescriptor, Location.None, target));
            return;
        }

        var icons = XamlCatalog.Read(ctx, files);

        ctx.AddSource("IconName.g.cs", SourceText.From(CatalogEmitter.Enum(ns, icons), Encoding.UTF8));
        ctx.AddSource("IconData.g.cs", SourceText.From(CatalogEmitter.Data(ns, icons), Encoding.UTF8));
        ctx.AddSource("IconCatalog.g.cs", SourceText.From(CatalogEmitter.Catalog(ns, icons), Encoding.UTF8));
        foreach (var icon in icons)
            ctx.AddSource($"Icons/{icon.Pascal}.g.cs", SourceText.From(CatalogEmitter.TypedControl(ns, icon), Encoding.UTF8));
    }

    private static string? NamespaceFor(string target)
    {
        if (string.Equals(target, "Maui", StringComparison.OrdinalIgnoreCase)) return "ShellIcons.Maui";
        if (string.Equals(target, "Avalonia", StringComparison.OrdinalIgnoreCase)) return "ShellIcons.Avalonia";
        return null;
    }
}
