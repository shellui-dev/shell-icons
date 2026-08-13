---
title: Sizing & stroke
description: Size, StrokeWidth, and AbsoluteStroke — all the ways to control icon dimensions.
order: 1
---

# Sizing & stroke

## `Size`

`Size` is a `string` — anything valid for an SVG `width`/`height` attribute. Default is `"24"`.

```razor:preview
<PreviewRow Align="end">
  <Zap Size="16" />
  <Zap Size="24" />
  <Zap Size="32" />
  <Zap Size="48" />
  <Zap Size="64" />
</PreviewRow>
```

CSS units work too:

```razor:preview
<PreviewRow FontSize="32px">
  <span>Text with <Zap Size="1em" /> a matched icon.</span>
</PreviewRow>
```

## `StrokeWidth`

Any `double`. Lucide's default is `2`.

```razor:preview
<PreviewRow>
  <Zap Size="40" StrokeWidth="0.75" />
  <Zap Size="40" StrokeWidth="1" />
  <Zap Size="40" StrokeWidth="1.5" />
  <Zap Size="40" StrokeWidth="2" />
  <Zap Size="40" StrokeWidth="3" />
</PreviewRow>
```

## `AbsoluteStroke`

By default, stroke scales with size — a 48px icon has a visually thicker stroke than a 16px one at the same `StrokeWidth="2"`. Sometimes that's not what you want; you want the *rendered* stroke width to look constant across sizes.

`AbsoluteStroke="true"` divides the stroke by size to compensate:

**Default (stroke scales):**

```razor:preview
<PreviewRow Align="end">
  <Zap Size="16" />
  <Zap Size="24" />
  <Zap Size="48" />
  <Zap Size="80" />
</PreviewRow>
```

**Absolute (stroke is visually constant):**

```razor:preview
<PreviewRow Align="end">
  <Zap Size="16" AbsoluteStroke />
  <Zap Size="24" AbsoluteStroke />
  <Zap Size="48" AbsoluteStroke />
  <Zap Size="80" AbsoluteStroke />
</PreviewRow>
```

The math: emitted `stroke-width = StrokeWidth × 24 / Size`. When `Size` isn't a plain number (`"1.5em"`, `"100%"`), `AbsoluteStroke` silently falls back to `StrokeWidth` as-is — there's no scale to compensate for.

## Sizing via CSS

Because everything ultimately becomes an SVG element, you can size icons via CSS too:

```razor:preview
<PreviewRow>
  <span style="display:inline-flex;width:2rem;height:2rem;color:royalblue">
    <Zap Size="100%" />
  </span>
</PreviewRow>
```

Set `Size="100%"` and let the parent constrain the box.
