---
title: IconCore
description: The base class every typed icon inherits. Owns the SVG root, prop plumbing, accessibility.
order: 1
---

# `IconCore`

Every typed icon inherits from `ShellIcons.IconCore`. It owns:

- The `<svg>` root element (all Lucide-contract attributes)
- Prop plumbing — `Size`, `StrokeWidth`, `Class`, `Title`, `AbsoluteStroke`
- The accessibility path — `role="img"` + `aria-labelledby` + `<title>` when `Title` is set; `aria-hidden="true"` otherwise
- Additional-attribute forwarding to the root SVG

You don't instantiate `IconCore` directly. Icon subclasses provide two things: the kebab-case `IconName` and the inner shape markup via `EmitChildren`.

## Parameters

| Prop | Type | Default | Description |
|---|---|---|---|
| `Size` | `string` | `"24"` | Width and height. Any SVG width value: `"24"`, `"1.5em"`, `"100%"`. |
| `StrokeWidth` | `double` | `2` | Root `stroke-width`. |
| `AbsoluteStroke` | `bool` | `false` | When true, keeps rendered stroke visually constant regardless of `Size`. Emitted `stroke-width` = `StrokeWidth × 24 / Size`. |
| `Class` | `string?` | `null` | Forwarded to the `<svg>` element as `class`. |
| `Title` | `string?` | `null` | Accessible name. When set: `role="img"` + `aria-labelledby` + a `<title>` child. When null: `aria-hidden="true"`. |
| `AdditionalAttributes` | `IReadOnlyDictionary<string, object>?` | `null` | Any unmatched attribute — `style`, `data-*`, `@onclick`, etc. — is forwarded to the `<svg>` root. |

## Emitted HTML

Given:

```razor
<ChevronRight Size="20" StrokeWidth="1.5" Class="text-primary" />
```

You get:

```html
<svg xmlns="http://www.w3.org/2000/svg"
     width="20" height="20"
     viewBox="0 0 24 24"
     fill="none"
     stroke="currentColor"
     stroke-width="1.5"
     stroke-linecap="round"
     stroke-linejoin="round"
     class="text-primary"
     aria-hidden="true">
  <path d="m9 18 6-6-6-6"/>
</svg>
```

Byte-identical to Lucide's SVG for the same icon, at the same version.

## Writing your own icon in code

You almost never need to — the generator handles the 1,555 Lucide icons and any drop-in from `catalog/custom/`. But if you want a one-off icon in application code:

```csharp
using Microsoft.AspNetCore.Components.Rendering;
using ShellIcons;

public sealed class MyBrandLogo : IconCore
{
    protected override string IconName => "my-brand-logo";

    protected override void EmitChildren(RenderTreeBuilder builder, int seq) =>
        builder.AddMarkupContent(seq, "<path d=\"…\"/>");
}
```

That's the whole contract. Two overrides, five lines.
