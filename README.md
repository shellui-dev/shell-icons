# ShellIcons

Lucide-derived SVG icons for **Blazor**, **Avalonia**, and **.NET MAUI**.

- Zero JS, zero required CSS
- Same visual system across all three .NET UI frameworks
- Native rendering per target: inline `<svg>` in Blazor, native `Path` shapes in Avalonia/MAUI
- Tree-shakeable typed components per icon

See [SHELLICONS.md](SHELLICONS.md) for the full design proposal.

## Status

**Phase 2 complete — full Lucide 0.475.0 catalog (1555 icons) auto-generated for Blazor.**

- [x] Design doc
- [x] Solution scaffold
- [x] Blazor `IconCore` + hand-generated proof-of-concept icons
- [x] Blazor sample app
- [x] Lucide catalog sync script (pinned via `LUCIDE_VERSION.txt`)
- [x] `catalog/custom/` drop-in path for repo-owned icons
- [x] Roslyn incremental source generator — reads both `catalog/lucide/` and `catalog/custom/`, emits 1555 typed components + `ShellIcon` dispatcher
- [ ] Trim smoke test in CI (~1 day)
- [ ] Publish `ShellIcons.Blazor 0.1.0-alpha` to NuGet (Phase 3)
- [ ] Avalonia target (Phase 4)
- [ ] MAUI target (Phase 5)

## Blazor quickstart

Not yet published. During development:

```xml
<ProjectReference Include="path/to/src/ShellIcons.Blazor/ShellIcons.Blazor.csproj" />
```

Typed icons (tree-shakeable) — import the `ShellIcons.Icons` namespace **per page** (imports live in a sub-namespace so common names like `Router` or `Activity` don't collide with Blazor/system types unless you opt in):

```razor
@using ShellIcons.Icons

<ChevronRight />
<Zap Size="16" StrokeWidth="1.5" />
<TriangleAlert Class="text-warning" />
```

Dispatcher (ships the full 1555-icon catalog — trimmer-hostile, use for dynamic lookups) — lives in `ShellIcons`:

```razor
@using ShellIcons

<ShellIcon Name="chevron-right" />
<ShellIcon Name="@page.IconName" Size="20" />
```

Colors inherit from CSS:

```html
<span style="color: crimson">
    <TriangleAlert Size="20" />
</span>
```

Accessibility:

```razor
<X Title="Close dialog" />
```

## Repo layout

```
shell-icons/
├── SHELLICONS.md              design proposal
├── LUCIDE_VERSION.txt         pinned upstream tag
├── NOTICE.md                  Lucide ISC attribution
├── ShellIcons.sln
├── Directory.Build.props
├── catalog/
│   ├── lucide/icons/          vendored Lucide SVGs (populated by sync-lucide.ps1)
│   └── custom/                repo-owned SVGs, drop-in
│       ├── README.md          drop-in contract
│       └── icons/
├── scripts/
│   └── sync-lucide.ps1        refresh vendored Lucide catalog
├── src/
│   └── ShellIcons.Blazor/     Razor Class Library
└── samples/
    └── ShellIcons.Blazor.Sample/
```

## Refreshing the Lucide catalog

```powershell
./scripts/sync-lucide.ps1                    # pinned version
./scripts/sync-lucide.ps1 -Version 0.480.0   # bump + repin
```

## Adding custom icons

Drop an SVG into [catalog/custom/icons/](catalog/custom/icons/) and rebuild — the Phase 2 source generator picks it up automatically and emits a component into `ShellIcons.Blazor`. Same 24×24 stroke contract as Lucide. See [catalog/custom/README.md](catalog/custom/README.md) for the drop-in contract.

Custom icons live alongside Lucide-derived icons in the same namespace; consumers can't tell them apart. Use them for:

- Fill variants of Lucide icons (active-state UI, e.g. `HouseFilled` next to `House`)
- Brand marks and product-specific glyphs
- Full overrides of a Lucide icon — custom wins on name collision

## Licensing

- ShellIcons: MIT (see [LICENSE](LICENSE))
- Lucide-pack icons: ISC (upstream), attribution in [NOTICE.md](NOTICE.md)
- Custom-pack icons: MIT (repo-owned, not covered by NOTICE)
