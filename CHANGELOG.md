# Changelog

All notable changes to the ShellIcons packages. `ShellIcons.Blazor` is published on NuGet; `ShellIcons.Maui` is in preview and not published yet.

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versioning follows [SemVer](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Blazor: `{Name}Icon` components** — `<ChevronRightIcon />`, `<ZapIcon />`, … in the root `ShellIcons` namespace. A single `@using ShellIcons` now works next to UI kits whose component names match Lucide icons (`Badge`, `Table`, `Router`, …), which previously failed with RZ9985. This is the recommended form for markup. Lucide's `shell` icon is the one exception, since `ShellIcon` is the dispatcher; use `Icon.Shell()` (build info `SHELLICONS003`).
- **Blazor: `Icon.*` factory** — `Icon.ChevronRight()` returns a `RenderFragment`, for icons as values: component parameters and lists built in C#. Unused icons are still trimmed.
- **Blazor: clear error for `@onclick` on an icon.** On a component, Razor passes `@onclick="…"` as a plain string, which broke rendering in the browser with a cryptic error. It now throws an `InvalidOperationException` that explains the forms that work.
- **`ShellIcons.Maui` (preview, not yet on NuGet)** — the same catalog as native MAUI `Path` shapes: typed controls (`<icons:ChevronRight />`), an `IconName` dispatcher (`<icons:Icon Name="Search" />`), and `IconCatalog` with tags, categories, alias-aware `TryParse` and `Search`. Builds for Android, iOS, Mac Catalyst and Windows.
- Build diagnostics for custom icons that can't be converted for MAUI: `SHELLICONS002` (unsupported SVG such as `<g>` or `transform`), `SHELLICONS004` (name not usable as an enum member), `SHELLICONS005` (shape outside the viewBox, dropped).
- Docs site at [shellicons.shellui.dev](https://shellicons.shellui.dev) (deployed from `main`), with a searchable icon browser and a MAUI section.

### Changed

- `Icon.*` calls with no arguments reuse a cached fragment instead of allocating on every render. `ShellIcons.Blazor.dll` is about a third smaller than the first factory version, even with the new `{Name}Icon` components.
- Components in `ShellIcons.Icons` are no longer `sealed`.
- The build no longer references `Microsoft.SourceLink.GitHub`, whose `Microsoft.Build.Tasks.Git 8.0.0` dependency has a known vulnerability (NU1902). Source Link now comes from the .NET SDK; packages still link to the exact commit.

### Fixed

- The README's click-handler example for `additionalAttributes` used the key `"@onclick"`; the working key is `"onclick"`.
- MAUI: Lucide 0.475.0's `save-off.svg` contains a stray shape outside the viewBox. Browsers clip it; on MAUI it's dropped so it can't draw beside the icon.

## [0.1.0-alpha] — 2026-08-22

First release of `ShellIcons.Blazor`: all 1,555 icons from Lucide 0.475.0 as typed components, the `<ShellIcon Name="…" />` dispatcher, `IconCore` with `Size`, `StrokeWidth`, `AbsoluteStroke`, `Class` and `Title`, and a drop-in path for custom icons.
