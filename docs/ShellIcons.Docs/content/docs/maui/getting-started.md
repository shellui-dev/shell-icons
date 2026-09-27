---
title: MAUI getting started
description: ShellIcons for .NET MAUI — the same 1,555 icons as native Path shapes.
order: 1
---

# ShellIcons for .NET MAUI

The same Lucide catalog as the Blazor package, drawn as native MAUI `Path` shapes. The SVG is converted at build time, so nothing parses SVG on the device.

<Callout Variant="warning" Title="Preview — not on NuGet yet">
ShellIcons.Maui builds for Android, iOS, Mac Catalyst and Windows and is covered by tests, but it hasn't been checked visually on Android or iOS devices yet. Reference the project from source until it's published.
</Callout>

## Install

Until the package is published, reference the project. You need the MAUI workloads installed (`dotnet workload install maui`).

```xml
<ProjectReference Include="path/to/shell-icons/src/ShellIcons.Maui/ShellIcons.Maui.csproj" />
```

Supported targets: `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`.

## Use in XAML

One namespace covers everything:

```xml
<ContentPage xmlns:icons="https://shellicons.dev/maui">
    <HorizontalStackLayout Spacing="8">
        <icons:ChevronRight Size="16" />
        <icons:Icon Name="Search" Color="Crimson" />
    </HorizontalStackLayout>
</ContentPage>
```

There are two forms:

| Form | Example | Use for |
|---|---|---|
| Typed control | `<icons:ChevronRight />` | Icons fixed in your layout. Each carries its own shape, so unused icons are trimmed. |
| `Icon` dispatcher | `<icons:Icon Name="Search" />` | Icons that are values — bindings, view-model properties, component parameters. Keeps the whole catalog. |

XAML prefixes keep names apart, so `<icons:Image />` (the icon) and `<Image />` (MAUI's control) coexist on the same page.

## Properties

| Property | Default | Notes |
|---|---|---|
| `Size` | `24` | Width and height |
| `StrokeThickness` | `2` | On Lucide's 24-unit grid; scales with `Size` |
| `AbsoluteStroke` | `false` | Keeps the rendered stroke at `StrokeThickness` at any size |
| `Color` | `null` | Stroke (and fill) color. Bindable, so `{DynamicResource …}` works |
| `Title` | `null` | Accessible description; when null the icon stays out of the accessibility tree |

## Color and theming

MAUI has no CSS `currentColor`, so icons don't inherit text color. With `Color` unset, an icon is black in the light theme and white in the dark theme. To follow your own palette everywhere, set it once with a style:

```xml
<Style TargetType="icons:IconView" ApplyToDerivedTypes="True">
    <Setter Property="Color" Value="{DynamicResource Foreground}" />
</Style>
```

`IconView` is the base of both the typed controls and `Icon`, so `ApplyToDerivedTypes` covers every icon.

## Taps and accessibility

Icons are `InputTransparent`: a tap goes to the button or row that hosts the icon, which is what you want in lists and toolbars. To make an icon clickable, put the gesture on its container:

```xml
<Border>
    <Border.GestureRecognizers>
        <TapGestureRecognizer Command="{Binding CloseCommand}" />
    </Border.GestureRecognizers>
    <icons:X Title="Close" />
</Border>
```

Set `Title` whenever the icon is the only thing conveying meaning, as in the icon-only button above.

## From C#

```csharp
using ShellIcons.Maui;

var chevron = new ShellIcons.Maui.Icons.ChevronRight { Size = 16 };
var status = new Icon { Name = IconName.CircleCheck, Color = Colors.Green };
```

The typed controls live in `ShellIcons.Maui.Icons` so names like `Image` or `Grid` don't clash with MAUI's own types in C#.

## Next

- [Icon names & catalog](/docs/maui/names-and-catalog) — `IconName`, aliases, search, bindings
- [How rendering works](/docs/maui/rendering) — SVG → `Path`, custom icons on MAUI
