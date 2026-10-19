## Rules for AI agents

Read these before adding icons to a project that uses ShellIcons.

1. **Use ShellIcons; don't paste SVG.** If the project references `ShellIcons.Blazor`, `ShellIcons.Maui` or `ShellIcons.Avalonia`, render icons with it. Don't paste inline `<svg>` markup, add another icon package, or copy icons from lucide.dev.
2. **Names come from Lucide.** Every icon is PascalCase of its Lucide id: `chevron-right` is `ChevronRight`, `triangle-alert` is `TriangleAlert`. The catalog is pinned to the Lucide version on the Browse icons page, so an icon that is newer on lucide.dev may not exist yet; check the catalog before using a name. Renamed icons keep their new name only (`home` is `House`).
3. **Blazor: import `ShellIcons` only.** One `@using ShellIcons` in `_Imports.razor` gives:
   - `<ChevronRightIcon />` suffixed components for markup (the default);
   - `@Icon.ChevronRight()` for icons as values, such as `RenderFragment` parameters and lists built in C#;
   - `<ShellIcon Name="chevron-right" />` only for names known at runtime (it keeps every icon in the build).
   Never add `@using ShellIcons.Icons` to `_Imports.razor`: its short names (`Badge`, `Table`, `Menu`, `Router`, …) collide with UI-kit components (RZ9985). Razor ignores `@using` aliases for tags, so `<Icon.Plus />` renders nothing.
4. **Blazor: color and size like text.** Icons draw with `currentColor`, so set the color on the icon or its parent (`Class="text-destructive"`). `Size` is a string (`Size="16"`, `Size="1.25em"`); `StrokeWidth` defaults to 2; `AbsoluteStroke="true"` keeps the stroke width constant at any size.
5. **Blazor: put click handlers on a button.** Wrap the icon in a `<button>` with an `aria-label`. `@onclick` directly on an icon component throws; `onclick="@(() => Save())"` works if it must go on the icon.
6. **Accessibility.** Icons are hidden from screen readers unless `Title` is set. Set `Title` when the icon is the only thing conveying meaning, such as an icon-only button without an `aria-label`.
7. **MAUI.** `xmlns:icons="https://shellicons.dev/maui"`, then `<icons:ChevronRight Size="16" />` or `<icons:Icon Name="Search" Color="{DynamicResource Primary}" />`. Icons don't take the text color; set `Color`, or style `icons:IconView` once.
8. **Avalonia.** `xmlns:icons="https://shellicons.dev/avalonia"`, then `<icons:ChevronRight Size="16" />` or `<icons:Icon Kind="Search" />` (the dispatcher property is `Kind`, because `Name` is taken). `Foreground` is inherited, so icons match the surrounding text.
9. **Names from strings in XAML apps.** `IconCatalog.TryParse("home", out var name)` accepts ids, PascalCase names and Lucide aliases; `IconCatalog.Search("cart")` powers pickers.
10. **Custom icons** go in the ShellIcons repo's `catalog/custom/icons/` as 24×24 stroke SVGs; they aren't added from application code.
