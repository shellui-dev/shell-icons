# ShellIcons

Lucide-derived SVG icons for **Blazor**, **Avalonia**, and **.NET MAUI**.

- Zero JS, zero required CSS
- Same visual system across all three .NET UI frameworks
- Native rendering per target: inline `<svg>` in Blazor, native `Path` shapes in Avalonia/MAUI
- Tree-shakeable typed components per icon

See [SHELLICONS.md](SHELLICONS.md) for the full design proposal.

## Status

**Docs live in dev, Phase 2 complete — full Lucide 0.475.0 catalog (1555 icons) auto-generated for Blazor.**

- [x] Design doc
- [x] Solution scaffold
- [x] Blazor `IconCore` with Size/StrokeWidth/Class/Title/AbsoluteStroke props
- [x] Lucide catalog sync script (pinned via `LUCIDE_VERSION.txt`)
- [x] `catalog/custom/` drop-in path for repo-owned icons
- [x] Roslyn incremental source generator — reads both `catalog/lucide/` and `catalog/custom/`, emits 1555 typed components + `ShellIcon` dispatcher
- [x] Test suite: 46 unit + integration tests (xUnit + bUnit)
- [x] Docs site at [docs/ShellIcons.Docs](docs/ShellIcons.Docs) — ShellDocs-powered, runs at `dotnet run` on http://localhost:5145
- [x] Live icon browser at `/icons` — searchable, 1555-cell grid with click-to-copy
- [x] GH Pages workflow + `CNAME` for [shellicons.shellui.dev](https://shellicons.shellui.dev)
- [x] CI + Release pipelines — see [RELEASING.md](RELEASING.md) for the runbook
- [ ] Blocker: `shelldocs.cli` build doesn't emit `index.html` or the WASM runtime — GH Pages deploy waits on a CLI fix upstream
- [ ] First publish `ShellIcons.Blazor 0.1.0-alpha` to NuGet — bump version, tag `v0.1.0-alpha`, approve in the Actions UI
- [ ] Avalonia target (Phase 4)
- [ ] MAUI target (Phase 5)

## Blazor quickstart

Not yet published. During development:

```xml
<ProjectReference Include="path/to/src/ShellIcons.Blazor/ShellIcons.Blazor.csproj" />
```

### Pick your API

Four ways to render an icon — all produce identical SVG. Only the first two are safe to use next to a UI kit.

| API | Import | Collides with UI kits? | Tree-shakeable? | Use for |
|---|---|---|---|---|
| **`<ChevronRightIcon />`** suffixed component | `@using ShellIcons` | ✅ No | ✅ Yes | **Default for markup.** Reads as an icon, full parameter binding. |
| **`@Icon.ChevronRight()`** factory | `@using ShellIcons` | ✅ No | ✅ Yes | **Icons as values** — `RenderFragment` parameters, nav-item lists, configs. |
| `<ShellIcon Name="chevron-right" />` dispatcher | `@using ShellIcons` | ✅ No | ❌ Roots all 1,555 | Names only known at runtime (CMS, JSON, markdown). |
| `<ChevronRight />` flat component | `@using ShellIcons.Icons` | ⚠️ Yes — RZ9985 on Badge, Table, Menu, Router, Card… | ✅ Yes | Icon-only files with no UI kit imported. |

One `@using ShellIcons` in `_Imports.razor` gives you the first three.

#### `<ChevronRightIcon />` — suffixed components (recommended for markup)

Every icon is also emitted as `{Name}Icon` in the root `ShellIcons` namespace. The suffix keeps names clear of UI-kit components and tells readers "this is an icon" at a glance — the same alias `lucide-react` ships.

```razor
@using ShellIcons

<ChevronRightIcon />
<ZapIcon Size="16" StrokeWidth="1.5" />
<TriangleAlertIcon Class="text-warning" />
<XIcon Title="Close dialog" />
```

One exception: Lucide's `shell` icon would be `ShellIcon`, which is the dispatcher, so it has no suffixed form — use `@Icon.Shell()`.

#### `@Icon.ChevronRight()` — factory (icons as values)

Every icon has a static method on `Icon` returning a `RenderFragment`. Use it wherever an icon is *data* rather than markup:

```razor
@* A component parameter typed RenderFragment *@
<NavItem Href="/" Label="Home" Icon="@Icon.House()" />

@* A list built in C# *@
@code {
    private readonly (string Label, RenderFragment Icon)[] _items =
    [
        ("Inbox",    Icon.Inbox()),
        ("Settings", Icon.Settings(size: "16")),
    ];
}
```

A call with no arguments returns a cached fragment, so `@Icon.Plus()` in a hot render path doesn't allocate.

#### `<ShellIcon Name="…" />` — dispatcher

Runtime lookup by kebab-case name, for icons whose identity is data-driven. Referencing it roots the full catalog, so prefer the two forms above when the name is known at compile time.

```razor
<ShellIcon Name="chevron-right" />
<ShellIcon Name="@page.IconName" Size="20" />
```

#### `<ChevronRight />` — flat components (⚠️ RZ9985 trap)

`@using ShellIcons.Icons` puts all 1,555 unprefixed names into your tag lookup. With a UI kit that has `Badge`, `Table`, `Router`, etc., you get RZ9985 collisions.

**Workarounds that don't work** (verified by real integrators):

- `@using Icon = ShellIcons.Icons` then `<Icon.Plus />` — Razor's tag matcher ignores namespace aliases; the tag compiles as an unknown HTML element and **renders blank**.
- Type aliases (`@using Badge = MyApp.UI.Badge`) — ignored by the tag matcher too.

Use the suffixed components instead, or fully qualify: `<ShellIcons.Icons.ChevronRight />`.

### Using icons inside component libraries

**Buttons and similar containers — just put the icon in the content.** No `Icon` slot needed; the component's CSS sizes child SVGs (shadcn's `[&_svg]:size-4`):

```razor
<Button><ChevronRightIcon /> Next</Button>
```

**Components that place the icon somewhere specific** (input adornments, alerts, nav items) take a `RenderFragment` parameter — pass the factory:

```razor
<Input StartIcon="@Icon.Search()" Placeholder="Search…" />
```

### Click handlers

Put handlers on a wrapping `<button>` — screen readers expect that. If you must attach one to the icon itself, drop the `@`:

```razor
<button @onclick="Save" aria-label="Save"><SaveIcon /></button>   @* ✅ recommended *@
<SaveIcon onclick="@(() => Save())" />                             @* ✅ works *@
<SaveIcon @onclick="Save" />                                       @* ❌ throws — see below *@
```

On a *component*, Razor passes `@onclick="Save"` as a plain string named `@onclick`; the browser then rejects that attribute name and the render batch fails. ShellIcons throws a clear `InvalidOperationException` instead. For the factory, the dictionary key is `"onclick"` (no `@`):

```razor
@Icon.Bell(additionalAttributes: new Dictionary<string, object>
{
    ["onclick"] = EventCallback.Factory.Create(this, HandleClick),
    ["data-testid"] = "notifications",
})
```

### Colors and accessibility

Colors inherit from CSS `color` (`stroke="currentColor"`):

```razor
<span style="color: crimson"><TriangleAlertIcon Size="20" /></span>
```

Without `Title` icons are decorative (`aria-hidden="true"`); with it they get `role="img"` + `<title>`:

```razor
<XIcon Title="Close dialog" />
```

## Repo layout

```
shell-icons/
├── SHELLICONS.md              design proposal
├── LUCIDE_VERSION.txt         pinned upstream tag
├── NOTICE.md                  Lucide ISC attribution
├── ShellIcons.slnx
├── Directory.Build.props
├── catalog/
│   ├── lucide/icons/          vendored Lucide SVGs (populated by sync-lucide.ps1)
│   └── custom/                repo-owned SVGs, drop-in
│       ├── README.md          drop-in contract
│       └── icons/
├── scripts/
│   └── sync-lucide.ps1        refresh vendored Lucide catalog
├── src/
│   ├── ShellIcons.Blazor/     Razor Class Library (net8.0;net9.0)
│   └── ShellIcons.Generator/  Roslyn incremental source generator (netstandard2.0)
├── tests/
│   ├── ShellIcons.Generator.Tests/   xUnit: SvgParser, Naming
│   └── ShellIcons.Blazor.Tests/       xUnit + bUnit: IconCore, generated icons, dispatcher
├── docs/
│   └── ShellIcons.Docs/       ShellDocs site — dev at http://localhost:5145
└── .github/workflows/
    └── docs.yml               GH Pages deploy pipeline (waits on shelldocs.cli fix)
```

## Docs

Run the docs locally:

```bash
cd docs/ShellIcons.Docs
dotnet run
```

Serves at http://localhost:5145. Pages live in `docs/ShellIcons.Docs/content/docs/*.md`. Live component previews via `razor:preview` code fences work out of the box.

The `/icons` route is a searchable browser of the entire catalog with click-to-copy component names.

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
