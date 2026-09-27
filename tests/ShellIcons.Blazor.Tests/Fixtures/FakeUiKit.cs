using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace FakeUiKit;

// Stand-in for a UI kit component that shares a name with a Lucide icon.
public sealed class Badge : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", "fake-badge");
        builder.AddContent(2, ChildContent);
        builder.CloseElement();
    }
}
