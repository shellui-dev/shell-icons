# ShellIcons.Avalonia (preview)

Lucide-derived icons for Avalonia, from the same 1,555-icon catalog as `ShellIcons.Blazor` and
`ShellIcons.Maui`. Docs: [shellicons.shellui.dev/docs/avalonia/getting-started](https://shellicons.shellui.dev/docs/avalonia/getting-started).

Not published yet. The release workflow only packs `ShellIcons.Blazor`.

Targets `net8.0` and Avalonia 12.1.

## Use

```xml
<Window xmlns:icons="https://shellicons.dev/avalonia">
    <StackPanel Orientation="Horizontal" Spacing="8">
        <!-- Typed control: carries its own path data, so the trimmer drops unused icons -->
        <icons:ChevronRight Size="16" />

        <!-- Dispatcher: the icon is a value, so it can come from a binding -->
        <icons:Icon Kind="Search" Foreground="Crimson" />
        <icons:Icon Kind="{Binding StatusIcon}" Title="Status" />
    </StackPanel>
</Window>
```

| Property | Default | Notes |
|---|---|---|
| `Size` | `24` | Width and height |
| `StrokeThickness` | `2` | On the 24-unit grid, scaled with `Size` like the other targets |
| `AbsoluteStroke` | `false` | Keep the rendered stroke at `StrokeThickness` at any size |
| `Foreground` | inherited | The same inherited property text uses, so an icon in a `Button` takes the button's foreground |
| `Title` | `null` | Sets `AutomationProperties.Name`; null keeps the icon out of the accessibility tree |

Names and metadata are the same API as the MAUI package:

```csharp
IconCatalog.TryParse("home", out var name);        // → IconName.House (Lucide alias)
IconCatalog.Get(IconName.House).Tags;               // ["home", "living", "building", …]
IconCatalog.Search("cart");                         // id matches first, then aliases/tags/categories
```

## Differences from ShellIcons.Maui

| | MAUI | Avalonia | Why |
|---|---|---|---|
| Dispatcher property | `Name` | `Kind` | Avalonia's `Name` is already the element name (`x:Name`) |
| Color | `Color` (a `Color`) | `Foreground` (a brush, inherited) | Avalonia has an inherited foreground, so icons follow text color the way `currentColor` does in Blazor |
| Hit testing | `InputTransparent` | `IsHitTestVisible="False"` | Each framework's own property; same behavior |

`IconName`, `IconCatalog`, `IconInfo`, `Size`, `StrokeThickness`, `AbsoluteStroke` and `Title` are identical.

## How it draws

`ShellIcons.Generator.Xaml` converts each SVG at build time into one normalized path string
(`M L C Q A Z`, absolute coordinates) — the same conversion and the same data the MAUI package uses.
Avalonia parses that grammar natively, so the control turns it into a `StreamGeometry` once per icon,
caches it, and draws it in `Render` under a scale transform. There are no child controls: an icon is
one visual.

## Design decisions

| Decision | Why |
|---|---|
| Custom-drawn `Control`, not a `Path` inside a template | One visual per icon, no template to apply, no layout pass for children. Lists and toolbars show many icons. |
| `Foreground` added from `TextElement.ForegroundProperty` | It inherits, so icons match surrounding text without a style. |
| `Kind` for the dispatcher | `Name` is taken; `Kind` is what Avalonia icon libraries already use. |
| Shared generator with MAUI, picked by `<ShellIconsTarget>` | One catalog reader and one emitter, so the two XAML packages can't drift apart. |

## Verified

- Builds with zero warnings; part of `ShellIcons.slnx`, so it needs no extra workloads.
- 33 headless tests that render through Skia: every icon parses, stays on the 24×24 grid and draws ink inside its own cell; size, stroke, color, XAML namespace and accessibility are checked from real pixels or the real property system.
- **Not yet verified:** a real desktop window on Windows, macOS or Linux. The headless renderer uses the same Skia backend, so differences are unlikely, but nobody has looked.

## Before publishing

- Run it once in a real Avalonia app on each desktop OS.
- Give the package its own readme — `Directory.Build.props` currently packs the repo's Blazor-centric `README.md` into every package.
- Add `ShellIcons.Avalonia` to the pack step and the "already published" check in `release.yml`. It packs on ubuntu, so no new job is needed.
