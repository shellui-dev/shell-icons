---
title: Typed vs dispatcher
description: When to use <ChevronRight /> vs <ShellIcon Name="chevron-right" />.
order: 5
---

# Typed vs dispatcher

ShellIcons ships two ways to render an icon. Both produce identical HTML. The difference is at build time.

> **Which typed form?** The examples below use the flat `<ChevronRight />` names from `ShellIcons.Icons`. In an app that also uses a UI kit, use the suffixed twins from `ShellIcons` instead — `<ChevronRightIcon />` — or the factory `@Icon.ChevronRight()`. They tree-shake the same way, and they can't collide with UI-kit components like `Badge` or `Table` (RZ9985).

## Typed form — the default

```razor
@using ShellIcons.Icons

<ChevronRight />
<Zap Size="16" />
```

- Each icon is a distinct C# type
- Blazor's IL trimmer sees which types you actually reference and drops the rest
- IntelliSense — start typing `<Ch` and get every icon starting with "Ch"
- Standard `[Parameter]` binding, standard everything

## Dispatcher form — the escape hatch

```razor
@using ShellIcons

<ShellIcon Name="chevron-right" />
<ShellIcon Name="@page.IconName" Size="20" />
```

- A single `ShellIcon` component with a `Name` string
- Backed by a static dictionary of all 1,555 icons the source generator populates
- Referencing `ShellIcon` **anywhere** in your app roots the entire catalog — the trimmer can no longer eliminate any of them

## The tradeoff, made concrete

| Scenario | Recommendation |
|---|---|
| Icon name is known at compile time — `<TriangleAlert />` on a button | **Typed** |
| Icon name comes from a markdown author, JSON payload, CMS field | **Dispatcher** |
| Icon name is chosen from a small compile-time set | **Typed** (with a `switch` if needed) |
| You want the smallest possible bundle | **Typed only** — don't reference `ShellIcon` |

## Mixing

Both can live in the same app. As long as you never reference `<ShellIcon />` in code the trimmer can reach, it and its dictionary get eliminated. The moment you do reference it — even once — the full catalog stays.

## When you need dynamic names, prefer a switch

If you have a handful of dynamic icons and want to keep tree-shaking, a switch beats the dispatcher:

```razor
@switch (kind)
{
    case "alert":   <TriangleAlert />; break;
    case "success": <CircleCheck />;   break;
    case "info":    <CircleAlert />;   break;
    default:        <CircleHelp />;    break;
}
```

Uses only the 4 typed icons — the other 1,551 get trimmed.

## `ShellIcon.Names`

The dispatcher exposes the full catalog for enumeration:

```csharp
var count = ShellIcon.Names.Count;                    // 1555
var contains = ShellIcon.Names.Contains("chevron-right"); // true
```

This is fine — accessing `Names` alone triggers the same "full catalog is rooted" behavior as rendering `<ShellIcon />`.
