---
title: Avalonia getting started
description: ShellIcons for Avalonia — the same 1,555 icons, drawn natively and colored like text.
order: 1
---

# ShellIcons for Avalonia

The same Lucide catalog as the Blazor and MAUI packages, drawn directly by Avalonia. The SVG is converted at build time, so nothing parses SVG in your app.

<Callout Variant="warning" Title="Preview — not on NuGet yet">
ShellIcons.Avalonia is covered by tests that render every icon through Avalonia's Skia backend, but it hasn't been run in a real desktop window yet. Reference the project from source until it's published.
</Callout>

## Install

Until the package is published, reference the project. No extra workloads are needed.

```xml
<ProjectReference Include="path/to/shell-icons/src/ShellIcons.Avalonia/ShellIcons.Avalonia.csproj" />
```

Targets `net8.0` and Avalonia 12.1.

## Use in XAML

One namespace covers everything:

```xml
<Window xmlns:icons="https://shellicons.dev/avalonia">
    <StackPanel Orientation="Horizontal" Spacing="8">
        <icons:ChevronRight Size="16" />
        <icons:Icon Kind="Search" Foreground="Crimson" />
    </StackPanel>
</Window>
```

There are two forms:

| Form | Example | Use for |
|---|---|---|
| Typed control | `<icons:ChevronRight />` | Icons fixed in your layout. Each carries its own shape, so unused icons are trimmed. |
| `Icon` dispatcher | `<icons:Icon Kind="Search" />` | Icons that are values — bindings and view-model properties. Keeps the whole catalog. |

The dispatcher's property is `Kind`, not `Name`: in Avalonia, `Name` is the element's own name.

XAML prefixes keep names apart, so `<icons:Image />` (the icon) and `<Image />` (Avalonia's control) coexist in the same file.

## Properties

| Property | Default | Notes |
|---|---|---|
| `Size` | `24` | Width and height |
| `StrokeThickness` | `2` | On Lucide's 24-unit grid; scales with `Size` |
| `AbsoluteStroke` | `false` | Keeps the rendered stroke at `StrokeThickness` at any size |
| `Foreground` | inherited | Stroke (and fill) brush |
| `Title` | `null` | Accessible name; when null the icon stays out of the accessibility tree |

## Color and theming

`Foreground` is the same inherited property that text uses. An icon takes the color of the text around it, like `currentColor` in the browser, and follows the theme with no extra work:

```xml
<Button Foreground="White" Background="RoyalBlue">
    <StackPanel Orientation="Horizontal" Spacing="6">
        <icons:Save Size="16" />      <!-- white, from the button -->
        <TextBlock Text="Save" />
    </StackPanel>
</Button>
```

Set it directly to override, or style every icon at once — `IconView` is the base of both the typed controls and `Icon`:

```xml
<Style Selector=":is(icons|IconView)">
    <Setter Property="Foreground" Value="{DynamicResource SystemAccentColorBrush}" />
</Style>
```

## Clicks and accessibility

Icons are not hit-test visible: a click goes to the button or row that hosts the icon. To make an icon clickable, put it in a `Button`:

```xml
<Button Command="{Binding CloseCommand}">
    <icons:X Title="Close" />
</Button>
```

Set `Title` whenever the icon is the only thing conveying meaning. It becomes the icon's automation name, and the icon is exposed to screen readers as an image.

## From C#

```csharp
using Avalonia.Media;
using ShellIcons.Avalonia;

var chevron = new ShellIcons.Avalonia.Icons.ChevronRight { Size = 16 };
var status = new Icon { Kind = IconName.CircleCheck, Foreground = Brushes.Green };
```

The typed controls live in `ShellIcons.Avalonia.Icons` so names like `Image` or `Grid` don't clash with Avalonia's own types in C#.

## How it draws

Each icon is a single custom-drawn control with no child elements. Its path data is the same build-time conversion the MAUI package uses (see [How rendering works](/docs/maui/rendering)); Avalonia turns it into a geometry once per icon and every control showing that icon shares it. Icons are drawn on Lucide's 24×24 grid and scaled to `Size`, so different icons of the same size line up.

## Next

- [Icon names & catalog](/docs/avalonia/names-and-catalog) — `IconName`, aliases, search, bindings
