---
title: How rendering works
description: How SVG icons become native MAUI Path shapes, and what that means for custom icons.
order: 3
---

# How rendering works

Blazor renders each icon's SVG directly. MAUI has no SVG support in XAML, so ShellIcons converts every icon to path data when the package is built.

## From SVG to one path

A source generator reads each SVG and turns every shape into part of **one** path, using absolute coordinates only:

| SVG | Becomes |
|---|---|
| `<path d="…">` | The same path, with relative commands made absolute and shorthand curves expanded |
| `<circle>`, `<ellipse>` | Two half-arcs |
| `<rect rx ry>` | Four lines and four arc corners |
| `<line>`, `<polyline>`, `<polygon>` | Straight segments (closed for a polygon) |

One path per icon means one native view per icon — lists and toolbars render many of them. Only the few Lucide shapes with `fill="currentColor"` get a second path, drawn filled and stroked like the original.

On the device, the control reads that pre-converted path once per icon and caches the geometry, so a hundred `ChevronRight`s share one shape.

## Sizing

Every icon is drawn on Lucide's 24×24 grid and scaled to `Size`. The grid, not each icon's own bounds, decides the scale, so a `minus` and a `circle-alert` of the same `Size` line up the way they do in Lucide. The stroke is scaled along with it unless you set `AbsoluteStroke`.

## Custom icons on MAUI

[Custom icons](/docs/guides/custom-icons) dropped into `catalog/custom/icons/` are converted too, as long as they follow the flat SVG contract. Blazor renders almost any SVG, so a custom icon can look right in the browser and still have problems on MAUI. The build tells you:

| You use | On MAUI | Diagnostic |
|---|---|---|
| `<g>`, `<use>`, gradients, masks, text | That part is skipped | `SHELLICONS002` warning |
| `transform="…"` | Reported — bake it into the coordinates | `SHELLICONS002` warning |
| A shape entirely outside `0 0 24 24` | Dropped — browsers clip it, a native path wouldn't | `SHELLICONS005` info |

Lucide itself has one of those: `save-off.svg` contains a stray shape outside the viewBox. It's invisible in a browser, and ShellIcons drops it on MAUI so it can't draw beside the icon.
