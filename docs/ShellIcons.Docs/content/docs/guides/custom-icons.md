---
title: Custom icons
description: Drop your own SVGs alongside Lucide's — they compile in as first-class typed components.
order: 4
---

# Custom icons

**Drop an SVG into `catalog/custom/icons/`. Rebuild. That's it.**

The Roslyn source generator scans both `catalog/lucide/icons/` (vendored) and `catalog/custom/icons/` (repo-owned) and emits typed components from both packs into the same `ShellIcons.Icons` namespace. Consumers can't tell them apart.

## The SVG contract

Match Lucide's format so custom icons feel visually cohesive:

```xml
<svg xmlns="http://www.w3.org/2000/svg"
     width="24" height="24" viewBox="0 0 24 24"
     fill="none"
     stroke="currentColor"
     stroke-width="2"
     stroke-linecap="round"
     stroke-linejoin="round">
  <!-- your shape elements here -->
</svg>
```

- **viewBox** must be `0 0 24 24`
- **stroke** must be `currentColor` — literal colors break theming
- **stroke-width** is `2`
- **stroke-linecap** and **stroke-linejoin** are `round`
- **Inner elements** — any of `<path>`, `<circle>`, `<rect>`, `<line>`, `<polyline>`, `<polygon>`

## File naming

`kebab-case.svg`. The file name becomes the component name:

| File                       | Component            | Dispatcher `Name`      |
|----------------------------|----------------------|------------------------|
| `house-filled.svg`         | `<HouseFilled />`    | `"house-filled"`       |
| `my-brand-logo.svg`        | `<MyBrandLogo />`    | `"my-brand-logo"`      |

## Fill variants — the classic use case

Lucide is stroke-only. When you want a filled icon for active-state UI, draw it as a custom icon:

```
catalog/custom/icons/house-filled.svg
```

```razor
@if (activePage == "home") { <HouseFilled /> } else { <House /> }
```

The filled variant should use `fill="currentColor"` on the shape elements that need to be solid.

## Optional metadata

Drop `<name>.json` next to the SVG:

```json
{
  "tags": ["home", "filled", "active-state"],
  "aliases": ["home-solid"],
  "categories": ["navigation"],
  "deprecated": false
}
```

Format mirrors Lucide's. Extra keys are ignored.

## Collision with Lucide

If your file name matches a Lucide icon (e.g. `catalog/custom/icons/zap.svg` while Lucide also has `zap.svg`), **custom wins**. The generator treats it as a deliberate override and emits a `SHELLICONS001` build-log message so the swap is visible.

Use this to swap out any Lucide icon you don't like without forking the whole catalog:

```
catalog/custom/icons/zap.svg   <!-- your preferred lightning bolt -->
```

`<Zap />` now renders your version everywhere.

## On MAUI

Custom icons are converted for [MAUI](/docs/maui/rendering) too. Keep them flat — no `<g>`, no `transform`, every shape inside the 24×24 grid — and they render the same on every target. The build warns (`SHELLICONS002`) when something won't convert.

## Licensing

Custom-pack icons are your own work under whatever license your project uses. They are **not** attributed in `NOTICE.md` — that file covers only the Lucide pack. If you fork this repo and add a lot of custom icons, treat them as first-class source code, not vendored content.
