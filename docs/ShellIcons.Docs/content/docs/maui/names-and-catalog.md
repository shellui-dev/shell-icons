---
title: Icon names & catalog
description: The IconName enum, Lucide aliases, and searching the catalog from C#.
order: 2
---

# Icon names & catalog

## `IconName`

Every icon has an `IconName` member: PascalCase of its Lucide file name (`chevron-right` → `ChevronRight`), the same rule the Blazor components use. `IconName.None` renders nothing.

Because it's an enum, an icon can be a view-model property:

```csharp
public partial class StatusViewModel : ObservableObject
{
    [ObservableProperty]
    private IconName _statusIcon = IconName.CircleCheck;
}
```

```xml
<icons:Icon Name="{Binding StatusIcon}" />
```

## Names from strings

Icons often arrive as strings — JSON config, a CMS, settings. `IconCatalog.TryParse` accepts a kebab-case id, a PascalCase name or a Lucide alias, ignoring case:

```csharp
IconCatalog.TryParse("chevron-right", out var a);   // IconName.ChevronRight
IconCatalog.TryParse("ChevronRight", out var b);    // IconName.ChevronRight
IconCatalog.TryParse("home", out var c);            // IconName.House — Lucide renamed home → house
IconCatalog.TryParse("nope", out var d);            // false, IconName.None
```

Aliases matter because Lucide renames icons between versions. Stored names like `home` keep working after the catalog is bumped. A real icon id always wins over another icon's alias.

## Metadata and search

Each icon carries the tags, categories and aliases from its Lucide JSON sidecar:

```csharp
IconInfo house = IconCatalog.Get(IconName.House);
// house.Id         "house"
// house.Source     "lucide" (or "custom")
// house.Tags       ["home", "living", "building", "residence", "architecture"]
// house.Categories ["buildings", "home"]
// house.Aliases    ["home"]
```

`IconCatalog.Search` powers icon pickers. Matches on the id come first, then aliases, tags and categories:

```csharp
foreach (var icon in IconCatalog.Search("cart"))
    Console.WriteLine(icon.Id);   // shopping-cart, … then icons tagged "cart"
```

`IconCatalog.All` lists every icon; `IconCatalog.Count` is the catalog size. Everything is built lazily on first use.
