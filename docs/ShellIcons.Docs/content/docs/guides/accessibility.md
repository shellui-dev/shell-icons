---
title: Accessibility
description: When Title is right, when it's wrong, and what ShellIcons emits under the hood.
order: 3
---

# Accessibility

Icons come in two flavors for screen readers: **decorative** (skipped) and **meaningful** (announced). ShellIcons makes both cases correct by default — you just choose which you want.

## Decorative — the default

No `Title` prop → the icon gets `aria-hidden="true"`. Screen readers skip it entirely.

Use this when the icon is redundant with visible text next to it:

```razor:preview
<PreviewRow>
  <button style="display:inline-flex;align-items:center;gap:0.5rem;padding:0.5rem 1rem;border-radius:6px">
    <TriangleAlert Size="16" /> Delete
  </button>
</PreviewRow>
```

The button announces as "Delete" — the icon adds nothing new for a screen-reader user, so hiding it is correct.

## Meaningful — with a `Title`

`Title="…"` → the icon becomes `role="img"` with an `aria-labelledby` pointing to a real `<title>` child:

```razor:preview
<X Title="Close dialog" Size="24" />
```

Rendered HTML:

```html
<svg role="img" aria-labelledby="shellicon-1" ...>
  <title id="shellicon-1">Close dialog</title>
  <path d="M18 6 6 18"/>
  <path d="m6 6 12 12"/>
</svg>
```

Screen readers announce "Close dialog, image."

Use this when the icon is the **only** content — a bare close button, a status indicator, a stat delta arrow.

## Rule of thumb

- Icon has visible text right next to it → **no `Title`** (decorative)
- Icon is the entire button/link/status → **`Title` is required**

Blazor's a11y linters won't catch the wrong choice — you have to think it through per-usage. Two-second cost, big correctness win.

## Under the hood

The `id` in `aria-labelledby` is generated at render time — `shellicon-{counter}`. Multiple titled icons on the same page get distinct IDs. This isn't something you configure — it's automatic.

## Interactive icons — buttons, not icons

If you find yourself wanting `@onclick` on an `<X />` directly, don't:

```razor
@* Wrong — the SVG isn't focusable, not keyboard-accessible *@
<X @onclick="Close" />
```

Wrap it in a `<button>`:

```razor
@* Right — real focusable, keyboard-activated, semantically a button *@
<button @onclick="Close" aria-label="Close">
    <X />
</button>
```

The `aria-label` on the button makes the icon inside redundant → leave it decorative.

## Additional attributes

Any attribute not consumed by ShellIcons is forwarded to the `<svg>` element:

```razor
<X data-testid="close-btn" style="opacity: 0.8" />
```
