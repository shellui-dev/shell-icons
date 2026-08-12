using Microsoft.AspNetCore.Components.Rendering;
using ShellIcons;

namespace ShellIcons.Blazor.Tests;

/// <summary>
/// A minimal <see cref="IconCore"/> subclass for isolating IconCore behavior in tests,
/// independent of anything the source generator emits.
/// </summary>
internal sealed class TestIcon : IconCore
{
    /// <summary>The path data used by the test icon. Simple triangle-ish path — content doesn't matter.</summary>
    public const string PathData = "m9 18 6-6-6-6";

    protected override string IconName => "test-icon";

    protected override void EmitChildren(RenderTreeBuilder builder, int seq) =>
        builder.AddMarkupContent(seq, $"<path d=\"{PathData}\"/>");
}
