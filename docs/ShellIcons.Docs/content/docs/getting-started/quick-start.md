---
title: Quick start
description: First icon in a Blazor page in under a minute.
order: 2
---

# Quick start

## 1. Install

```bash
dotnet add package ShellIcons.Blazor
```

## 2. Import the namespace

Once, in `Components/_Imports.razor`:

```razor
@using ShellIcons
```

## 3. Render icons

Every icon is a component named `{Name}Icon`. IntelliSense the name you want — `<Chevron` will suggest `ChevronDownIcon`, `ChevronLeftIcon`, `ChevronRightIcon`, `ChevronUpIcon`, and so on.

```razor:preview
<PreviewRow>
  <ChevronLeftIcon />
  <ChevronRightIcon />
  <ChevronDownIcon />
  <ChevronUpIcon />
</PreviewRow>
```

When the icon is a *value* — a `RenderFragment` parameter on another component, or an item in a list — use the factory instead:

```razor
<NavItem Href="/" Label="Home" Icon="@Icon.House()" />
```

## 4. Size and stroke

```razor:preview
<PreviewRow Align="end">
  <Zap Size="16" />
  <Zap Size="24" />
  <Zap Size="32" />
  <Zap Size="48" />
</PreviewRow>
```

`Size` accepts any SVG width value:

```razor:preview
<PreviewRow FontSize="24px">
  <span>inline <Zap Size="1.5em" /> with 24px text</span>
</PreviewRow>
```

Stroke width via `StrokeWidth`:

```razor:preview
<PreviewRow>
  <Zap Size="40" StrokeWidth="0.75" />
  <Zap Size="40" StrokeWidth="1" />
  <Zap Size="40" StrokeWidth="1.5" />
  <Zap Size="40" StrokeWidth="2" />
  <Zap Size="40" StrokeWidth="3" />
</PreviewRow>
```

## 5. Color via CSS

Every icon uses `stroke="currentColor"`. Set `color` on any ancestor — no `Color` prop:

```razor:preview
<PreviewRow Gap="1.5rem">
  <PreviewCell Color="crimson"><TriangleAlert Size="32" /></PreviewCell>
  <PreviewCell Color="seagreen"><CircleCheck Size="32" /></PreviewCell>
  <PreviewCell Color="royalblue"><Search Size="32" /></PreviewCell>
</PreviewRow>
```

## 6. Accessibility

By default icons are decorative (`aria-hidden="true"`). Add `Title` to make them announce to screen readers:

```razor:preview
<X Title="Close dialog" Size="24" />
```

## 7. Look up icons by string (dispatcher)

For dynamic use cases — icon names from JSON, markdown-authored content, CMS-driven UI — use the dispatcher:

```razor:preview
<PreviewRow>
  <ShellIcon Name="pizza" Size="32" />
  <ShellIcon Name="cat" Size="32" />
  <ShellIcon Name="ghost" Size="32" />
  <ShellIcon Name="skull" Size="32" />
  <ShellIcon Name="crown" Size="32" />
</PreviewRow>
```

**Tradeoff:** the dispatcher roots the full catalog, so it defeats Blazor's tree-shaking. Prefer typed icons for static usage; reach for the dispatcher when the icon name is a runtime value.

## Next

- [Sizing & stroke](/docs/guides/sizing-and-stroke) — details on `AbsoluteStroke`, CSS units
- [Color & theming](/docs/guides/color-and-theming) — dark mode, CSS variables
- [Accessibility](/docs/guides/accessibility) — when `Title` is right, when it's wrong
- [Browse the catalog](/icons) — search all 1,555 icons
