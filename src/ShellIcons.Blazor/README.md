![ShellIcons](https://raw.githubusercontent.com/shellui-dev/shell-icons/main/assets/readme-logo.svg)

# ShellIcons.Blazor

1,555 Lucide-derived icons for Blazor as inline `<svg>`: zero JS, zero required CSS, typed components that trim per icon. Docs: [shellicons.shellui.dev](https://shellicons.shellui.dev).

## Install

```bash
dotnet add package ShellIcons.Blazor
```

Targets `net8.0` and `net9.0` (and `net10.0` via forward compatibility). No CSS import, no JS.

## Pick your API

Four ways to render an icon — all produce identical SVG. Only the first two are safe to use next to a UI kit.

| API | Import | Collides with UI kits? | Tree-shakeable? | Use for |
|---|---|---|---|---|
| **`<ChevronRightIcon />`** suffixed component | `@using ShellIcons` | ✅ No | ✅ Yes | **Default for markup.** Reads as an icon, full parameter binding. |
| **`@Icon.ChevronRight()`** factory | `@using ShellIcons` | ✅ No | ✅ Yes | **Icons as values** — `RenderFragment` parameters, nav-item lists, configs. |
| `<ShellIcon Name="chevron-right" />` dispatcher | `@using ShellIcons` | ✅ No | ❌ Roots all 1,555 | Names only known at runtime (CMS, JSON, markdown). |
| `<ChevronRight />` flat component | `@using ShellIcons.Icons` | ⚠️ Yes — RZ9985 on Badge, Table, Menu, Router, Card… | ✅ Yes | Icon-only files with no UI kit imported. |

One `@using ShellIcons` in `_Imports.razor` gives you the first three.

### `<ChevronRightIcon />` — suffixed components (recommended for markup)

Every icon is also emitted as `{Name}Icon` in the root `ShellIcons` namespace. The suffix keeps names clear of UI-kit components and tells readers "this is an icon" at a glance — the same alias `lucide-react` ships.

```razor
@using ShellIcons

<ChevronRightIcon />
<ZapIcon Size="16" StrokeWidth="1.5" />
<TriangleAlertIcon Class="text-warning" />
<XIcon Title="Close dialog" />
```

One exception: Lucide's `shell` icon would be `ShellIcon`, which is the dispatcher, so it has no suffixed form — use `@Icon.Shell()`.

### `@Icon.ChevronRight()` — factory (icons as values)

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

### `<ShellIcon Name="…" />` — dispatcher

Runtime lookup by kebab-case name, for icons whose identity is data-driven. Referencing it roots the full catalog, so prefer the two forms above when the name is known at compile time.

```razor
<ShellIcon Name="chevron-right" />
<ShellIcon Name="@page.IconName" Size="20" />
```

### `<ChevronRight />` — flat components (⚠️ RZ9985 trap)

`@using ShellIcons.Icons` puts all 1,555 unprefixed names into your tag lookup. With a UI kit that has `Badge`, `Table`, `Router`, etc., you get RZ9985 collisions.

**Workarounds that don't work** (verified by real integrators):

- `@using Icon = ShellIcons.Icons` then `<Icon.Plus />` — Razor's tag matcher ignores namespace aliases; the tag compiles as an unknown HTML element and **renders blank**.
- Type aliases (`@using Badge = MyApp.UI.Badge`) — ignored by the tag matcher too.

Use the suffixed components instead, or fully qualify: `<ShellIcons.Icons.ChevronRight />`.

## Using icons inside component libraries

**Buttons and similar containers — just put the icon in the content.** No `Icon` slot needed; the component's CSS sizes child SVGs (shadcn's `[&_svg]:size-4`):

```razor
<Button><ChevronRightIcon /> Next</Button>
```

**Components that place the icon somewhere specific** (input adornments, alerts, nav items) take a `RenderFragment` parameter — pass the factory:

```razor
<Input StartIcon="@Icon.Search()" Placeholder="Search…" />
```

## Click handlers

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

## Colors and accessibility

Colors inherit from CSS `color` (`stroke="currentColor"`):

```razor
<span style="color: crimson"><TriangleAlertIcon Size="20" /></span>
```

Without `Title` icons are decorative (`aria-hidden="true"`); with it they get `role="img"` + `<title>`:

```razor
<XIcon Title="Close dialog" />
```

## Licensing

MIT. Lucide-derived icons are ISC; attribution is in `NOTICE.md` inside the package.
