<div align="center">

<picture>
  <source media="(prefers-color-scheme: dark)" srcset="assets/logo-dark.svg">
  <source media="(prefers-color-scheme: light)" srcset="assets/logo-light.svg">
  <img alt="ShellIcons logo" src="assets/logo-light.svg" width="120">
</picture>

# ShellIcons

Lucide-derived icons for **Blazor**, **.NET MAUI** and **Avalonia**.

[Docs](https://shellicons.shellui.dev) · [NuGet](https://www.nuget.org/packages?q=ShellIcons) · [Changelog](CHANGELOG.md)

</div>

- One catalog (1,555 Lucide 0.475.0 icons plus drop-in custom icons), the same names on every target
- Native rendering per target: inline `<svg>` in Blazor, native `Path` shapes in MAUI, direct drawing in Avalonia, all converted at build time
- Typed components per icon that trim, plus a dispatcher when the icon is a value
- Zero JS and zero required CSS in Blazor
- Docs for AI agents: [`/llms.txt`](https://shellicons.shellui.dev/llms.txt), every page as Markdown, and `AGENTS.md` / `CLAUDE.md` / Copilot / Cursor rule files ([AI Agents](https://shellicons.shellui.dev/docs/ai-agents))

## Packages

| Package | Status | Readme |
|---|---|---|
| `ShellIcons.Blazor` | [![NuGet](https://img.shields.io/nuget/vpre/ShellIcons.Blazor)](https://www.nuget.org/packages/ShellIcons.Blazor) | [src/ShellIcons.Blazor](src/ShellIcons.Blazor/README.md) |
| `ShellIcons.Maui` | [![NuGet](https://img.shields.io/nuget/vpre/ShellIcons.Maui)](https://www.nuget.org/packages/ShellIcons.Maui) preview | [src/ShellIcons.Maui](src/ShellIcons.Maui/README.md) |
| `ShellIcons.Avalonia` | [![NuGet](https://img.shields.io/nuget/vpre/ShellIcons.Avalonia)](https://www.nuget.org/packages/ShellIcons.Avalonia) preview | [src/ShellIcons.Avalonia](src/ShellIcons.Avalonia/README.md) |

## Quickstart

Blazor:

```bash
dotnet add package ShellIcons.Blazor
```

```razor
@using ShellIcons

<ChevronRightIcon />
<ZapIcon Size="16" StrokeWidth="1.5" />
<ShellIcon Name="@page.IconName" />
```

.NET MAUI:

```bash
dotnet add package ShellIcons.Maui --prerelease
```

```xml
<ContentPage xmlns:icons="https://shellicons.dev/maui">
    <icons:ChevronRight Size="16" />
    <icons:Icon Name="{Binding StatusIcon}" Color="{DynamicResource Primary}" />
</ContentPage>
```

Avalonia:

```bash
dotnet add package ShellIcons.Avalonia --prerelease
```

```xml
<Window xmlns:icons="https://shellicons.dev/avalonia">
    <icons:ChevronRight Size="16" />
    <icons:Icon Kind="{Binding StatusIcon}" Foreground="{DynamicResource AccentBrush}" />
</Window>
```

Avalonia's dispatcher uses `Kind` because every Avalonia control already has `Name`. The full guides are in each package's readme and on the [docs site](https://shellicons.shellui.dev).

## Repo layout

```
shell-icons/
├── CHANGELOG.md                release notes
├── RELEASING.md                how to cut a release
├── LUCIDE_VERSION.txt         pinned upstream tag
├── NOTICE.md                  Lucide ISC attribution
├── ShellIcons.slnx             Blazor, Avalonia, generators, tests — no MAUI workload needed
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
│   ├── ShellIcons.Avalonia/        Avalonia library (preview, net8.0)
│   └── ShellIcons.Generator.Xaml/  MAUI + Avalonia source generator, SVG → path conversion
├── tests/
│   ├── ShellIcons.Generator.Tests/  xUnit: SvgParser, Naming, path conversion
│   ├── ShellIcons.Blazor.Tests/     xUnit + bUnit: components, factory, dispatcher, collisions
│   ├── ShellIcons.Maui.Tests/       headless MAUI: controls, XAML, whole-catalog geometry
│   └── ShellIcons.Avalonia.Tests/   headless Avalonia with Skia: controls, XAML, renders of every icon
└── .github/workflows/
    ├── ci.yml                 ubuntu: ShellIcons.slnx · windows: ShellIcons.Maui.slnx
    └── release.yml            tag → all three packages on NuGet (Trusted Publishing) + GitHub Release
```

## Docs

The docs site, [shellicons.shellui.dev](https://shellicons.shellui.dev), lives in its own repo: [shellui-dev/shell-icons-docs](https://github.com/shellui-dev/shell-icons-docs). It uses the published packages, so update it after a release.

## Refreshing the Lucide catalog

```powershell
./scripts/sync-lucide.ps1                    # pinned version
./scripts/sync-lucide.ps1 -Version 0.480.0   # bump + repin
```

## Adding custom icons

Drop an SVG into [catalog/custom/icons/](catalog/custom/icons/) and rebuild — the source generators pick it up and emit it into every package. Same 24×24 stroke contract as Lucide. See [catalog/custom/README.md](catalog/custom/README.md) for the drop-in contract.

Custom icons live alongside Lucide-derived icons in the same namespace; consumers can't tell them apart. Use them for:

- Fill variants of Lucide icons (active-state UI, e.g. `HouseFilled` next to `House`)
- Brand marks and product-specific glyphs
- Full overrides of a Lucide icon — custom wins on name collision

## Licensing

- ShellIcons: MIT (see [LICENSE](LICENSE))
- Lucide-pack icons: ISC (upstream), attribution in [NOTICE.md](NOTICE.md)
- Custom-pack icons: MIT (repo-owned, not covered by NOTICE)
