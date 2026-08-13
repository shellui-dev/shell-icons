---
title: Catalog & source generator
description: How the source generator turns vendored SVGs into 1,555 typed components at build time.
order: 3
---

# Catalog & source generator

The typed icon components you use (`<ChevronRight />` and friends) aren't hand-written — they're emitted by a Roslyn source generator at every build. This page explains how it works and what to touch if you're contributing to the repo.

## Two source packs

```
catalog/
├── lucide/               <- vendored, refreshed by scripts/sync-lucide.ps1
│   └── icons/
│       ├── chevron-right.svg
│       ├── chevron-right.json   (Lucide metadata)
│       └── ...
└── custom/               <- repo-owned, hand-authored
    └── icons/
        ├── house-filled.svg     (optional — none by default)
        └── ...
```

Both feed the same generator. Both produce components in the same `ShellIcons.Icons` namespace. Consumers can't tell an icon from Lucide apart from an icon from custom.

## The generator pipeline

1. `ShellIcons.Blazor.csproj` marks `catalog/lucide/icons/*.svg` and `catalog/custom/icons/*.svg` as `<AdditionalFiles>`.
2. `ShellIcons.Generator` reads them via `context.AdditionalTextsProvider`.
3. For each SVG:
   - `SvgParser.ExtractInner` strips the outer `<svg>` wrapper, preserving the inner `<path>`/`<circle>`/etc. verbatim.
   - `Naming.KebabToPascal` turns `chevron-right` into `ChevronRight`.
   - `ResolvePack` tags the icon `"lucide"` or `"custom"` based on file path.
4. Collision resolution — custom-pack icons win over same-name Lucide icons. A `SHELLICONS001` info diagnostic is logged.
5. Emit — one `.g.cs` per icon plus a single `ShellIcon.g.cs` dispatcher:

```csharp
// Icons/ChevronRight.g.cs (auto-generated)
public sealed class ChevronRight : global::ShellIcons.IconCore
{
    protected override string IconName => "chevron-right";
    protected override void EmitChildren(RenderTreeBuilder builder, int seq) =>
        builder.AddMarkupContent(seq, @"<path d=""m9 18 6-6-6-6"" />");
}
```

## Refreshing the Lucide catalog

`LUCIDE_VERSION.txt` at the repo root pins the upstream tag. To bump:

```powershell
./scripts/sync-lucide.ps1                    # sync at pinned version
./scripts/sync-lucide.ps1 -Version 0.480.0   # bump + repin
```

The script downloads the Lucide GitHub archive at that tag, extracts `icons/`, replaces `catalog/lucide/icons/` in one atomic step, and updates `LUCIDE_VERSION.txt`.

Rebuild after. The generator picks up the diff automatically — new icons emit, changed icons emit updated shape data, dropped icons vanish.

## Diagnostics

| ID | Severity | When |
|---|---|---|
| `SHELLICONS001` | Info | A `catalog/custom/` icon has the same name as a `catalog/lucide/` icon. Custom wins; the message logs the override. |

## Debugging the generator

Set `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` in `ShellIcons.Blazor.csproj` and rebuild. Generated files land at `obj/Debug/net9.0/generated/ShellIcons.Generator/…/Icons/*.g.cs`. Compare the emitted code against your expectations if things don't render.

## Naming edge cases

- Lucide 0.475.0 ships **no numeric-prefix icons** (e.g. `1st-place-medal`), so we don't ship a NumberPrefix map yet. If a future Lucide bump introduces one, we'll port Lucide's own map — `"1st" → "firstPlace"`.
- Aliases (e.g. `home` → `house`) are not currently emitted as separate components. The alias metadata in the JSON sidecar is parsed but not used yet. Planned.

## Trimming

Both forms behave well under the trimmer:

- Typed icons — only referenced types survive
- `ShellIcon` dispatcher — referencing it (or `ShellIcon.Names`) roots the entire dictionary
