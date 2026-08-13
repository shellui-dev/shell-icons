---
title: Color & theming
description: Every icon uses currentColor. Coloring, dark mode, CSS variables.
order: 2
---

# Color & theming

**Every icon uses `stroke="currentColor"`.** There is no `Color` prop, on purpose — coloring inherits from the ancestor's CSS `color`.

## Inline color

```razor:preview
<PreviewRow Gap="1.5rem">
  <PreviewCell Color="crimson"><TriangleAlert Size="32" /></PreviewCell>
  <PreviewCell Color="seagreen"><CircleCheck Size="32" /></PreviewCell>
  <PreviewCell Color="royalblue"><Search Size="32" /></PreviewCell>
  <PreviewCell Color="rebeccapurple"><Zap Size="32" /></PreviewCell>
</PreviewRow>
```

## Via a class

```razor
<TriangleAlert Class="text-warning" />
```

Any CSS framework (Tailwind, Bootstrap utilities, your own) works — the class just needs to set `color` somewhere in the cascade.

## CSS variables

Icons pick up any custom property that resolves to `color`:

```css
:root {
    --icon-color: #2563eb;
}
.icon-slot {
    color: var(--icon-color);
}
```

```razor
<span class="icon-slot"><Zap Size="24" /></span>
```

## Dark mode

Because coloring comes from CSS, dark mode is a native concern — icons follow the theme without any prop toggling.

```css
:root { --foreground: oklch(0.15 0 0); }
:root.dark { --foreground: oklch(0.95 0 0); }

.icon { color: var(--foreground); }
```

## Multi-color / gradients

Lucide is intentionally monochrome. If you need a two-tone or gradient icon:

1. Draw a filled version as a [custom icon](/docs/guides/custom-icons) and use `fill="currentColor"` on the fill shapes.
2. Wrap the icon in a container that provides a colored background chip:

```razor:preview
<PreviewRow>
  <span style="display:inline-flex;align-items:center;justify-content:center;
               width:2.5rem;height:2.5rem;border-radius:9999px;
               background:oklch(0.94 0.03 264);color:oklch(0.4 0.15 264)">
    <Zap Size="18" />
  </span>
</PreviewRow>
```

3. For true fills/duotone across a whole set, watch for the future `ShellIcons.Blazor.Phosphor` package.
