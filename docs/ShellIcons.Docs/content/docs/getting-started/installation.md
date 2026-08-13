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

## Namespace layout

ShellIcons splits into two namespaces so importing them globally doesn't collide with anything Blazor or the BCL ships:

| Namespace | What lives there | When to import |
|---|---|---|
| `ShellIcons` | `IconCore`, `ShellIcon` dispatcher | Import globally in `_Imports.razor` |
| `ShellIcons.Icons` | All 1,555 typed icon components | Import **per page** — protects Blazor's `Router`, `System.Diagnostics.Activity`, `List<T>`, etc. from being shadowed |

**Global** — add to `Components/_Imports.razor`:

```razor
@using ShellIcons
```

**Per page** — at the top of any `.razor` file that uses typed icons:

```razor
@page "/dashboard"
@using ShellIcons.Icons

<ChevronRight />
```

## Verify

Drop this anywhere and run:

```razor:preview
<Zap Size="32" />
```

If you see a lightning bolt above, you're wired.

## Next

- [Quick start](/docs/getting-started/quick-start) — the whole 30-second tour
