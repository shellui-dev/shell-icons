![ShellIcons](https://raw.githubusercontent.com/shellui-dev/shell-icons/main/assets/readme-logo.svg)

# ShellIcons.Maui (preview)

Lucide-derived icons for .NET MAUI as native `Path` shapes, from the same 2,015-icon catalog as
`ShellIcons.Blazor`. Docs: [shellicons.shellui.dev/docs/maui/getting-started](https://shellicons.shellui.dev/docs/maui/getting-started).

## Install

```bash
dotnet add package ShellIcons.Maui --prerelease
```

Targets `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst` and `net10.0-windows10.0.19041.0`.

## Use

```xml
<ContentPage xmlns:icons="https://shellicons.dev/maui">
    <HorizontalStackLayout Spacing="8">
        <!-- Typed control: carries its own path data, so the trimmer drops unused icons -->
        <icons:ChevronRight Size="16" />

        <!-- Dispatcher: the icon is a value, so it can come from a binding or a parameter -->
        <icons:Icon Name="Search" Color="{DynamicResource Primary}" />
        <icons:Icon Name="{Binding StatusIcon}" Title="Status" />
    </HorizontalStackLayout>
</ContentPage>
```

| Property | Default | Notes |
|---|---|---|
| `Size` | `24` | Width and height |
| `StrokeThickness` | `2` | On the 24-unit grid, scaled with `Size` like the Blazor target |
| `AbsoluteStroke` | `false` | Keep the rendered stroke at `StrokeThickness` at any size |
| `Color` | `null` | Bindable. Null means black in light theme, white in dark |
| `Title` | `null` | Sets `SemanticProperties.Description`; null keeps the icon out of the accessibility tree |

Set a default color app-wide with a style:

```xml
<Style TargetType="icons:IconView" ApplyToDerivedTypes="True">
    <Setter Property="Color" Value="{DynamicResource Foreground}" />
</Style>
```

Names and metadata:

```csharp
IconCatalog.TryParse("home", out var name);        // → IconName.House (Lucide alias)
IconCatalog.Get(IconName.House).Tags;               // ["home", "living", "building", …]
IconCatalog.Search("cart");                         // id matches first, then aliases/tags/categories
```

## How it maps SVG to MAUI

`ShellIcons.Generator.Xaml` converts each SVG at build time. Every shape element becomes a
sub-path of one normalized path string — absolute commands only (`M L C Q A Z`), 3-decimal
coordinates:

| SVG | Normalized as |
|---|---|
| `<path d>` | Tokenized (compact numbers, exponents, compact arc flags), relative → absolute, `H`/`V` → `L`, `S` → `C`, `T` → `Q`; arc radii too small to reach their endpoint are scaled up as SVG requires |
| `<circle>` / `<ellipse>` | Two half-arcs |
| `<rect rx ry>` | Lines + four arc corners (SVG radius rules: missing radius copies the other, clamped to half a side) |
| `<line>` / `<polyline>` / `<polygon>` | `M` + `L…` (+ `Z`) |
| `fill="currentColor"` shapes | A second path that is filled and stroked |
| Shapes entirely outside the viewBox | Dropped (`SHELLICONS005`) — browsers clip them, a native `Path` wouldn't |

At runtime the control walks that pre-normalized string into a `PathGeometry` at the icon's size and
caches it, so every control showing the same icon at the same size shares one geometry.

## Design decisions

| Decision | Why |
|---|---|
| Path data ships as compact normalized strings, turned into geometry once per icon and size, and cached | Emitting per-icon construction code would be several times larger than the data. There's still no SVG parsing and no `PathGeometryConverter` at runtime. |
| Geometry is built at the icon's size, not scaled with a `RenderTransform` | MAUI moves a `Stretch.None` path whose bounds come within half a stroke of its view's edge, and it measures the untransformed geometry against the view. With a transform, large icons were pushed off-centre and clipped. |
| One `Path` per icon; a second only for `fill="currentColor"` shapes | Lists and toolbars show many icons, so one native view per icon matters. |
| Typed controls **and** an `IconName` dispatcher | Typed controls trim per icon; the enum lets an icon be a binding or a component parameter. |
| `Color`, not `TintColor` | In MAUI, `TintColor` usually means image tinting. `Color` is bindable, so `{DynamicResource …}` works. |
| Defaults match the Blazor package | Size 24, stroke 2, round caps and joins — icons look the same on both targets. |
| No consumer-selected icon subsets yet | The generator runs when this package is built, not in the consumer's build, so it can't see which icons an app wants. Typed controls already trim unused icons. |
| Same `IconName` / `Icon` names as ShellUI Native's interim icon component | ShellUI Native can switch to this package without changing its components. |

The conversion contract for custom icons is in [catalog/custom/README.md](https://github.com/shellui-dev/shell-icons/blob/main/catalog/custom/README.md); both generators share one naming rule (`Naming.KebabToPascal`).

## Verified

- Builds with zero warnings for `net10.0`, `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0` (on Windows, with the MAUI workloads).
- Tests on the plain `net10.0` build, including a compiled XAML fixture (xmlns, string → `IconName`, `icons:Image` next to MAUI's `Image`).
- A check that parses every icon into MAUI geometry and keeps every endpoint on the 24×24 grid. It caught a stray off-canvas shape in Lucide's own `save-off.svg`.
- Rendered from the packed package in a Windows app and on an Android emulator (API 35): sizes, colours, filled shapes, former names and taps passing through to the container. Not yet looked at on iOS.
- A check that every icon keeps half a default stroke away from the edges of its 24×24 grid, so MAUI never moves it.
