# Changelog

All notable changes to the ShellIcons packages: `ShellIcons.Blazor`, `ShellIcons.Maui` and `ShellIcons.Avalonia` (the last two in preview).

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versioning follows [SemVer](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0-alpha] — 2026-10-22

First release of `ShellIcons.Maui` and `ShellIcons.Avalonia` on NuGet, next to `ShellIcons.Blazor`. All three share one version and carry the ShellIcons mark as their package icon.

### Added

- **Blazor: `{Name}Icon` components** — `<ChevronRightIcon />`, `<ZapIcon />`, … in the root `ShellIcons` namespace. A single `@using ShellIcons` now works next to UI kits whose component names match Lucide icons (`Badge`, `Table`, `Router`, …), which previously failed with RZ9985. This is the recommended form for markup. Lucide's `shell` icon is the one exception, since `ShellIcon` is the dispatcher; use `Icon.Shell()` (build info `SHELLICONS003`).
- **Blazor: `Icon.*` factory** — `Icon.ChevronRight()` returns a `RenderFragment`, for icons as values: component parameters and lists built in C#. Unused icons are still trimmed.
- **Blazor: clear error for `@onclick` on an icon.** On a component, Razor passes `@onclick="…"` as a plain string, which broke rendering in the browser with a cryptic error. It now throws an `InvalidOperationException` that explains the forms that work.
- **`ShellIcons.Maui` (preview)** — the same catalog as native MAUI `Path` shapes: typed controls (`<icons:ChevronRight />`), an `IconName` dispatcher (`<icons:Icon Name="Search" />`), and `IconCatalog` with tags, categories, alias-aware `TryParse` and `Search`. Builds for Android, iOS, Mac Catalyst and Windows.
- **`ShellIcons.Avalonia` (preview)** — the same catalog for Avalonia 12 (`net8.0`): typed controls (`<icons:ChevronRight />`), a dispatcher (`<icons:Icon Kind="Search" />`) and the same `IconName` / `IconCatalog` API as the MAUI package. Icons inherit `Foreground`, so they take the color of the surrounding text.
- Build diagnostics for custom icons that can't be converted for MAUI or Avalonia: `SHELLICONS002` (unsupported SVG such as `<g>` or `transform`), `SHELLICONS004` (name not usable as an enum member), `SHELLICONS005` (shape outside the viewBox, dropped).
- Docs site at [shellicons.shellui.dev](https://shellicons.shellui.dev) (deployed from `main`), with a searchable icon browser, MAUI and Avalonia sections, and docs for AI agents (`/llms.txt`).

### Changed

- Each package ships its own readme on NuGet, in plain Markdown so nuget.org renders it; the root `README.md` is for GitHub.
- The release workflow runs on Windows with the MAUI workload and publishes all three packages.

- `Icon.*` calls with no arguments reuse a cached fragment instead of allocating on every render. `ShellIcons.Blazor.dll` is about a third smaller than the first factory version, even with the new `{Name}Icon` components.
- Components in `ShellIcons.Icons` are no longer `sealed`.
- The build no longer references `Microsoft.SourceLink.GitHub`, whose `Microsoft.Build.Tasks.Git 8.0.0` dependency has a known vulnerability (NU1902). Source Link now comes from the .NET SDK; packages still link to the exact commit.

### Fixed

- The README's click-handler example for `additionalAttributes` used the key `"@onclick"`; the working key is `"onclick"`.
- MAUI: Lucide 0.475.0's `save-off.svg` contains a stray shape outside the viewBox. Browsers clip it; on MAUI it's dropped so it can't draw beside the icon.

## [0.1.0-alpha] — 2026-08-22

First release of `ShellIcons.Blazor`: all 1,555 icons from Lucide 0.475.0 as typed components, the `<ShellIcon Name="…" />` dispatcher, `IconCore` with `Size`, `StrokeWidth`, `AbsoluteStroke`, `Class` and `Title`, and a drop-in path for custom icons.
