namespace ShellIcons.Generator;

/// <summary>
/// Normalized icon definition parsed from a Lucide SVG (or custom SVG that matches the same contract).
/// </summary>
internal sealed class IconDefinition
{
    public IconDefinition(string kebabName, string pascalName, string innerMarkup, string sourcePack)
    {
        KebabName = kebabName;
        PascalName = pascalName;
        InnerMarkup = innerMarkup;
        SourcePack = sourcePack;
    }

    public string KebabName { get; }     // "chevron-right"
    public string PascalName { get; }    // "ChevronRight"
    public string InnerMarkup { get; }   // "<path d=\"m9 18 6-6-6-6\"/>"
    public string SourcePack { get; }    // "lucide" or "custom"
}
