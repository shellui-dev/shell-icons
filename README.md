![ShellIcons](https://raw.githubusercontent.com/shellui-dev/shell-icons/main/assets/readme-logo.svg)

# ShellIcons

Lucide-derived SVG icons for **Blazor**, **Avalonia**, and **.NET MAUI**.

- Zero JS, zero required CSS
- Same visual system across all three .NET UI frameworks
- Native rendering per target: inline `<svg>` in Blazor, native `Path` shapes in Avalonia/MAUI
- Tree-shakeable typed components per icon

Docs: [shellicons.shellui.dev](https://shellicons.shellui.dev)

## Status

| Package | Status |
|---|---|
| `ShellIcons.Blazor` | **0.1.0-alpha on [NuGet](https://www.nuget.org/packages/ShellIcons.Blazor)** — 1,555 Lucide 0.475.0 icons |
| `ShellIcons.Maui` | **Preview, unpublished** — builds for Android, iOS, Mac Catalyst and Windows; see [src/ShellIcons.Maui](src/ShellIcons.Maui/README.md) |
| `ShellIcons.Avalonia` | Planned — will reuse the MAUI path conversion |

- [x] Source generators emit every icon from the vendored catalog (`LUCIDE_VERSION.txt`) plus drop-in `catalog/custom/` icons
- [x] Blazor: suffixed components (`<ChevronRightIcon />`), `Icon.*` factory, `<ShellIcon Name>` dispatcher — safe next to UI kits
- [x] MAUI: typed controls, `IconName` dispatcher, `IconCatalog` metadata with alias lookup
- [x] 135 tests (xUnit + bUnit + headless MAUI), CI for both solutions, tag-driven release — see [RELEASING.md](RELEASING.md)
- [x] Docs site at [docs/ShellIcons.Docs](docs/ShellIcons.Docs) (ShellDocs 0.1.7-alpha), with a searchable icon browser
- [x] Static build for GitHub Pages ([shellicons.shellui.dev](https://shellicons.shellui.dev)) — deploys on push to `main`
- [ ] MAUI: check rendering on Android/iOS devices, then publish
- [ ] Avalonia target

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

## MAUI quickstart (preview)

```xml
<ContentPage xmlns:icons="https://shellicons.dev/maui">
    <icons:ChevronRight Size="16" />
    <icons:Icon Name="{Binding StatusIcon}" Color="{DynamicResource Primary}" />
</ContentPage>
```

Not on NuGet yet — reference `src/ShellIcons.Maui` from source. Docs: [MAUI getting started](docs/ShellIcons.Docs/content/docs/maui/getting-started.md).

## Repo layout

```
shell-icons/
├── CHANGELOG.md                release notes
├── RELEASING.md                how to cut a release
├── LUCIDE_VERSION.txt         pinned upstream tag
├── NOTICE.md                  Lucide ISC attribution
├── ShellIcons.slnx             Blazor, generators, tests, docs — no MAUI workload needed
├── ShellIcons.Maui.slnx        MAUI projects — needs `dotnet workload install maui`
├── Directory.Build.props
├── catalog/
│   ├── lucide/icons/          vendored Lucide SVGs (populated by sync-lucide.ps1)
│   └── custom/                repo-owned SVGs, drop-in
│       ├── README.md          drop-in contract
│       └── icons/
├── scripts/
│   └── sync-lucide.ps1        refresh vendored Lucide catalog
├── src/
│   ├── ShellIcons.Blazor/          Razor Class Library (net8.0;net9.0)
│   ├── ShellIcons.Generator/       Blazor source generator (netstandard2.0)
│   ├── ShellIcons.Maui/            MAUI library (preview)
│   └── ShellIcons.Generator.Xaml/  MAUI source generator + SVG → path conversion
├── tests/
│   ├── ShellIcons.Generator.Tests/  xUnit: SvgParser, Naming, path conversion
│   ├── ShellIcons.Blazor.Tests/     xUnit + bUnit: components, factory, dispatcher, collisions
│   └── ShellIcons.Maui.Tests/       headless MAUI: controls, XAML, whole-catalog geometry
├── docs/
│   └── ShellIcons.Docs/       ShellDocs site — dev at http://localhost:5145
└── .github/workflows/
    ├── ci.yml                 ubuntu: ShellIcons.slnx · windows: ShellIcons.Maui.slnx
    ├── release.yml            tag → NuGet (Trusted Publishing) + GitHub Release
    └── docs.yml               static build → GitHub Pages
```

## Docs

Run the docs locally:

```bash
cd docs/ShellIcons.Docs
dotnet run
```

Serves at http://localhost:5145. Pages live in `docs/ShellIcons.Docs/content/docs/*.md`. Live component previews via `razor:preview` code fences work out of the box.

`/docs/icons` is a searchable browser of the entire catalog with click-to-copy component names.

Build the static site the way the deploy workflow does (needs `dotnet tool install -g ShellDocs.CLI --version 0.1.7-alpha`):

```bash
cd docs/ShellIcons.Docs
shelldocs build --output ../../publish --spa-fallback --site-url https://shellicons.shellui.dev
```

Every page under `content/` is prerendered to plain HTML, so any static host can serve `publish/`. Pages added as Razor `@page` routes outside `content/` are not prerendered — put new pages in `content/` (a component can be used from markdown, as `content/docs/icons.md` does).

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
