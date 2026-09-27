# Custom icons — drop-in contract

Icons you drop into [`icons/`](icons/) here are picked up by the Phase 2 source generator and emitted as first-class components in every target package (`ShellIcons.Blazor`, `ShellIcons.Avalonia`, `ShellIcons.Maui`). No wiring needed — just drop the SVG, rebuild.

## The SVG contract

Match Lucide's shape contract so custom icons are visually cohesive with the rest of the catalog:

```svg
<svg xmlns="http://www.w3.org/2000/svg"
     width="24" height="24" viewBox="0 0 24 24"
     fill="none"
     stroke="currentColor"
     stroke-width="2"
     stroke-linecap="round"
     stroke-linejoin="round">
  <!-- your paths here -->
</svg>
```

- **viewBox:** `0 0 24 24`
- **stroke:** `currentColor` — never a literal color
- **stroke-width:** `2`
- **stroke-linecap / stroke-linejoin:** `round`
- **fill:** `none` on the root, unless a specific shape needs a fill (fill variants use `fill="currentColor"` on the elements that should be filled)
- **Inner children:** any of `<path>`, `<circle>`, `<ellipse>`, `<rect>`, `<line>`, `<polyline>`, `<polygon>`. The generator strips the outer `<svg>` and preserves the shapes.

## Staying portable to MAUI (and Avalonia)

Blazor renders your SVG as-is, so almost anything works there. The XAML targets convert every shape to native path geometry at build time, and they only understand the flat contract above. Break it and the icon still works in Blazor, but the build reports a problem for MAUI:

| You use | What happens for MAUI | Diagnostic |
|---|---|---|
| `<g>`, `<use>`, `<defs>`, gradients, masks, text | That element is skipped | `SHELLICONS002` (warning) |
| `transform="…"` on any shape | Reported; bake the transform into the coordinates instead | `SHELLICONS002` (warning) |
| A shape entirely outside `0 0 24 24` | Dropped — browsers clip it, a native `Path` wouldn't | `SHELLICONS005` (info) |
| `fill` other than `none` | Drawn as a separate filled + stroked path | — |

Keep every shape inside the 24×24 grid, with no groups and no transforms, and the icon renders identically on every target.

## File naming

`kebab-case.svg`. The file name becomes the icon identity:

| SVG file                | Blazor component | Dispatcher `Name` |
|-------------------------|------------------|-------------------|
| `house-filled.svg`      | `<HouseFilled />`| `"house-filled"`  |
| `my-brand-logo.svg`     | `<MyBrandLogo />`| `"my-brand-logo"` |
| `1st-place-medal.svg`   | `<FirstPlaceMedal />` | `"1st-place-medal"` |

Numeric-prefix names go through the same map as Lucide (`1st` → `firstPlace`, etc.), so `<1stPlaceMedal />` — invalid C# — becomes `<FirstPlaceMedal />`.

## Optional metadata sidecar

Drop `<name>.json` alongside the SVG to add tags, aliases, or a deprecation marker. Format mirrors Lucide's:

```json
{
  "tags": ["home", "filled", "active-state"],
  "aliases": ["home-solid"],
  "categories": ["navigation"],
  "deprecated": false
}
```

Sidecar is optional. Extra keys are ignored.

## Collision with Lucide

If your custom icon has the same name as a Lucide-derived one (e.g. `catalog/custom/icons/zap.svg` while Lucide also ships `zap.svg`), **custom wins** — the generator treats it as a deliberate override. A build warning `SHELLICONS001` is emitted so the override is visible in logs.

Use this to swap out any Lucide icon you don't like without forking the whole catalog.

## Licensing

Custom-pack icons are your own work, covered by the repo's MIT license. They are **not** attributed in `NOTICE.md` — `NOTICE.md` only covers the Lucide pack.

## Real-world examples

**Fill variant for active-state UI:**

```
catalog/custom/icons/house-filled.svg
```

```razor
@if (activePage == "home") { <HouseFilled /> } else { <House /> }
```

**Brand mark:**

```
catalog/custom/icons/shellui-mark.svg
```

```razor
<ShellUiMark Size="24" />
```

**Override:**

```
catalog/custom/icons/zap.svg   <!-- your preferred lightning bolt -->
```

`<Zap />` now renders your version everywhere — build log shows `SHELLICONS001: 'zap' from custom overrides Lucide`.
