# Changelog

All notable changes to the ShellIcons packages: `ShellIcons.Blazor`, `ShellIcons.Maui` and `ShellIcons.Avalonia` (the last two in preview).

Format based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versioning follows [SemVer](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

The catalog moves from Lucide 0.475.0 to **Lucide 1.66.0**: 2,015 icons, 547 of them new. Lucide 1.x renamed 69 icons ShellIcons shipped and removed 18; renamed icons keep working under their old names.

### Added

- **547 new icons** from Lucide 1.x, on every target.
- **Former names stay usable.** Every name Lucide lists as an alias of an icon (usually its name before a rename) gets the same forms as the icon: `<Trash2Icon />`, `<Trash2 />`, `@Icon.Trash2()`, `<ShellIcon Name="trash-2" />` in Blazor, and `IconName.Trash2` plus `<icons:Trash2 />` in MAUI and Avalonia. They render the renamed icon, and the ones Lucide marks deprecated are `[Obsolete]` with a message naming the replacement, so the compiler points at what to change. `IconName.Trash2` is hidden from IntelliSense, and the `Icon` control reports `IconName.Trash`.
- `IconCatalog` tags and categories include the ones Lucide 1.x shares between icons (`$extends:` and `$group:` in its metadata), so `Search("done")` finds `circle-check`, `alarm-clock-check` and the rest of the check group.

### Changed

- **69 icons have new names.** The common ones: `trash-2` → `trash`, `filter` → `funnel`, `circle-help` → `circle-question-mark`, `history` → `rotate-ccw-clock`, `smile` → `face-slightly-smiling`, `music` → `eighth-notes`, `bold` → `letter-b`, `copyright` → `circle-letter-c`, `align-left` → `text-align-start`. The old names still compile, with an obsolete warning. The full list is below.
- **`Trash` draws a different icon.** Lucide's new `trash` is the old `trash-2` (with the two lines inside); the old plain `trash` is gone. `Trash2` and `Trash` now look the same.
- Several icons were redrawn upstream, for example `circle-check`'s tick.
- Typed controls in `ShellIcons.Maui.Icons` and `ShellIcons.Avalonia.Icons` are no longer `sealed`; former names derive from them.
- `IconCatalog.TryParse` matches exact case first, so `LetterS` and `Letters` (two icons whose names differ only by case) each resolve to themselves.
- `scripts/sync-lucide.ps1` also copies Lucide's tag groups into `catalog/lucide/groups/`.

### Removed

- **18 icons Lucide dropped**, mostly brand logos it removed for trademark reasons: `chrome`, `codepen`, `codesandbox`, `dribbble`, `facebook`, `figma`, `framer`, `github`, `gitlab`, `instagram`, `linkedin`, `pocket`, `rail-symbol`, `slack`, `trello`, `twitch`, `twitter`, `youtube`. Code using them no longer compiles. For brand logos, use a brand icon set, or add the SVG to `catalog/custom/icons/` if you have the right to use it.

### Fixed

- Two icons whose names differ only by case (`letter-s` and `letters`) made both generators fail; generated files are now named by icon id.

<details>
<summary>All 69 renamed icons</summary>

| Before | Now |
|---|---|
| `album` (`Album`) | `square-bookmark` (`SquareBookmark`) |
| `align-center` (`AlignCenter`) | `text-align-center` (`TextAlignCenter`) |
| `align-justify` (`AlignJustify`) | `text-align-justify` (`TextAlignJustify`) |
| `align-left` (`AlignLeft`) | `text-align-start` (`TextAlignStart`) |
| `align-right` (`AlignRight`) | `text-align-end` (`TextAlignEnd`) |
| `angry` (`Angry`) | `face-angry` (`FaceAngry`) |
| `annoyed` (`Annoyed`) | `face-expressionless` (`FaceExpressionless`) |
| `archive-restore` (`ArchiveRestore`) | `archive-arrow-up` (`ArchiveArrowUp`) |
| `badge-help` (`BadgeHelp`) | `badge-question-mark` (`BadgeQuestionMark`) |
| `bold` (`Bold`) | `letter-b` (`LetterB`) |
| `book-marked` (`BookMarked`) | `book-bookmark` (`BookBookmark`) |
| `building-2` (`Building2`) | `building-complex` (`BuildingComplex`) |
| `circle-help` (`CircleHelp`) | `circle-question-mark` (`CircleQuestionMark`) |
| `circle-parking` (`CircleParking`) | `circle-letter-p` (`CircleLetterP`) |
| `circle-parking-off` (`CircleParkingOff`) | `circle-letter-p-off` (`CircleLetterPOff`) |
| `copyright` (`Copyright`) | `circle-letter-c` (`CircleLetterC`) |
| `file-audio` (`FileAudio`) | `file-headphone` (`FileHeadphone`) |
| `file-audio-2` (`FileAudio2`) | `file-headphone` (`FileHeadphone`) |
| `file-badge-2` (`FileBadge2`) | `file-badge` (`FileBadge`) |
| `file-check-2` (`FileCheck2`) | `file-check-corner` (`FileCheckCorner`) |
| `file-code-2` (`FileCode2`) | `file-code-corner` (`FileCodeCorner`) |
| `file-json` (`FileJson`) | `file-braces` (`FileBraces`) |
| `file-json-2` (`FileJson2`) | `file-braces-corner` (`FileBracesCorner`) |
| `file-key-2` (`FileKey2`) | `file-key` (`FileKey`) |
| `file-lock-2` (`FileLock2`) | `file-lock` (`FileLock`) |
| `file-minus-2` (`FileMinus2`) | `file-minus-corner` (`FileMinusCorner`) |
| `file-plus-2` (`FilePlus2`) | `file-plus-corner` (`FilePlusCorner`) |
| `file-question` (`FileQuestion`) | `file-question-mark` (`FileQuestionMark`) |
| `file-search-2` (`FileSearch2`) | `file-search-corner` (`FileSearchCorner`) |
| `file-type-2` (`FileType2`) | `file-type-corner` (`FileTypeCorner`) |
| `file-video` (`FileVideo`) | `file-play` (`FilePlay`) |
| `file-video-2` (`FileVideo2`) | `file-video-camera` (`FileVideoCamera`) |
| `file-volume-2` (`FileVolume2`) | `file-signal` (`FileSignal`) |
| `file-warning` (`FileWarning`) | `file-exclamation-point` (`FileExclamationPoint`) |
| `file-x-2` (`FileX2`) | `file-x-corner` (`FileXCorner`) |
| `filter` (`Filter`) | `funnel` (`Funnel`) |
| `filter-x` (`FilterX`) | `funnel-x` (`FunnelX`) |
| `fingerprint` (`Fingerprint`) | `fingerprint-pattern` (`FingerprintPattern`) |
| `flip-horizontal` (`FlipHorizontal`) | `square-centerline-dashed-horizontal` (`SquareCenterlineDashedHorizontal`) |
| `flip-horizontal-2` (`FlipHorizontal2`) | `triangles-centerline-dashed-horizontal` (`TrianglesCenterlineDashedHorizontal`) |
| `flip-vertical` (`FlipVertical`) | `square-centerline-dashed-vertical` (`SquareCenterlineDashedVertical`) |
| `flip-vertical-2` (`FlipVertical2`) | `triangles-centerline-dashed-vertical` (`TrianglesCenterlineDashedVertical`) |
| `frown` (`Frown`) | `face-slightly-frowning` (`FaceSlightlyFrowning`) |
| `grab` (`Grab`) | `hand-grab` (`HandGrab`) |
| `heading` (`Heading`) | `letter-h` (`LetterH`) |
| `history` (`History`) | `rotate-ccw-clock` (`RotateCcwClock`) |
| `indent-decrease` (`IndentDecrease`) | `list-indent-decrease` (`ListIndentDecrease`) |
| `indent-increase` (`IndentIncrease`) | `list-indent-increase` (`ListIndentIncrease`) |
| `laugh` (`Laugh`) | `face-grinning` (`FaceGrinning`) |
| `letter-text` (`LetterText`) | `text-initial` (`TextInitial`) |
| `mail-question` (`MailQuestion`) | `mail-question-mark` (`MailQuestionMark`) |
| `meh` (`Meh`) | `face-neutral` (`FaceNeutral`) |
| `message-circle-question` (`MessageCircleQuestion`) | `message-circle-question-mark` (`MessageCircleQuestionMark`) |
| `music` (`Music`) | `eighth-notes` (`EighthNotes`) |
| `music-2` (`Music2`) | `eighth-note` (`EighthNote`) |
| `music-3` (`Music3`) | `half-note` (`HalfNote`) |
| `music-4` (`Music4`) | `sixteenth-notes` (`SixteenthNotes`) |
| `podcast` (`Podcast`) | `mic-signal` (`MicSignal`) |
| `shield-question` (`ShieldQuestion`) | `shield-question-mark` (`ShieldQuestionMark`) |
| `smile` (`Smile`) | `face-slightly-smiling` (`FaceSlightlySmiling`) |
| `smile-plus` (`SmilePlus`) | `face-slightly-smiling-plus` (`FaceSlightlySmilingPlus`) |
| `square-m` (`SquareM`) | `square-letter-m` (`SquareLetterM`) |
| `square-parking` (`SquareParking`) | `square-letter-p` (`SquareLetterP`) |
| `square-parking-off` (`SquareParkingOff`) | `square-letter-p-off` (`SquareLetterPOff`) |
| `text` (`Text`) | `text-align-start` (`TextAlignStart`) |
| `text-select` (`TextSelect`) | `square-dashed-text` (`SquareDashedText`) |
| `trash-2` (`Trash2`) | `trash` (`Trash`) |
| `waves` (`Waves`) | `waves-horizontal` (`WavesHorizontal`) |
| `wrap-text` (`WrapText`) | `text-wrap` (`TextWrap`) |

</details>

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
