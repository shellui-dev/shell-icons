---
title: ShellIcon dispatcher
description: <ShellIcon Name="…" /> — look up any icon by kebab-case name at runtime.
order: 2
---

# `<ShellIcon />` dispatcher

For dynamic use cases, `<ShellIcon Name="chevron-right" />` renders any icon in the catalog by name.

## Usage

```razor
@using ShellIcons

<ShellIcon Name="chevron-right" />
<ShellIcon Name="@page.IconName" Size="20" />
<ShellIcon Name="triangle-alert" StrokeWidth="2.5" Title="Warning" />
```

Inherits every parameter from [`IconCore`](/docs/reference/icon-core) — `Size`, `StrokeWidth`, `AbsoluteStroke`, `Class`, `Title`, plus additional-attribute splat.

## `Name`

`string`, kebab-case, matches Lucide's file names — `"chevron-right"`, `"triangle-alert"`, `"a-arrow-down"`.

Unknown names render an empty `<svg>` (no crash, no exception). If you'd rather guard:

```razor
@if (ShellIcon.Names.Contains(candidate))
{
    <ShellIcon Name="@candidate" />
}
else
{
    <CircleHelp />
}
```

## `ShellIcon.Names`

Static `IReadOnlyCollection<string>` — every icon name in the catalog, ordered.

```csharp
var count = ShellIcon.Names.Count;                       // 1555
var exists = ShellIcon.Names.Contains("chevron-right");  // true
foreach (var name in ShellIcon.Names) { … }
```

Useful for search UIs, config validation, catalog dumps.

## Trimming caveat

**Referencing `ShellIcon` anywhere roots the full catalog** — all 1,555 icons ship in your assembly, regardless of which ones you actually render.

If you care about trimmed size, use [typed icons](/docs/guides/typed-vs-dispatcher) instead. Reach for the dispatcher only when the icon name genuinely can't be known at compile time.

## When the dispatcher is the right answer

- **Markdown-authored content** — the author writes `<ShellIcon Name="rocket" />` in a `.md` file. Names are only known at content-load time.
- **CMS-driven UI** — an icon name comes back with each card/tag/badge from the database.
- **Icon-picker UIs** — you're literally letting a user search and pick from the catalog.
- **Config-driven feature flags** — a JSON config file specifies which icon appears next to each menu item.

## When the dispatcher is the wrong answer

- **Fixed icons in your UI** — `<ChevronRight />` is always right for a "next" button. Use the typed component.
- **A small dynamic set** — a switch over 3-5 known kinds. Use typed icons.
- **Blazor WASM apps where startup size matters** — the dispatcher adds ~50-100KB of catalog data to your bundle.
