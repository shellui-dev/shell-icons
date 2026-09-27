---
title: Installation
description: Add ShellIcons.Blazor to your project.
order: 1
---

# Installation

## NuGet

```bash
dotnet add package ShellIcons.Blazor
```

That's the whole install. No CSS import, no JS reference, no config file.

## Supported targets

- **`net8.0`** (LTS)
- **`net9.0`**
- **`net10.0`** works via forward-compat

## Import

Add one line to `Components/_Imports.razor`:

```razor
@using ShellIcons
```

That brings in everything you normally need, none of which collides with UI-kit components:

| Form | Example | Use for |
|---|---|---|
| Suffixed components | `<ChevronRightIcon />` | Markup — the default |
| Factory | `@Icon.ChevronRight()` | Icons as values (`RenderFragment` parameters, lists) |
| Dispatcher | `<ShellIcon Name="chevron-right" />` | Names only known at runtime |

The unsuffixed components (`<ChevronRight />`) live in `ShellIcons.Icons`. Don't import that namespace globally: its 1,555 short names collide with UI-kit components such as `Badge`, `Table` and `Router` (RZ9985). See [Typed vs dispatcher](/docs/guides/typed-vs-dispatcher).

## Verify

Drop this anywhere and run:

```razor:preview
<ZapIcon Size="32" />
```

If you see a lightning bolt above, you're wired.

## Next

- [Quick start](/docs/getting-started/quick-start) — the whole 30-second tour
