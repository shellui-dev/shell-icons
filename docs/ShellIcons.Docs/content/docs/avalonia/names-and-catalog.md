---
title: Icon names & catalog
description: The IconName enum, Lucide aliases, and searching the catalog from C# on Avalonia.
order: 2
---

# Icon names & catalog

`IconName`, `IconCatalog` and `IconInfo` are generated from the same source as the MAUI package, so the API is identical — only the namespace (`ShellIcons.Avalonia`) differs.

## `IconName`

Every icon has an `IconName` member: PascalCase of its Lucide file name (`chevron-right` → `ChevronRight`). `IconName.None` renders nothing.

Because it's an enum, an icon can be a view-model property:

```csharp
public partial class StatusViewModel : ObservableObject
{
    [ObservableProperty]
    private IconName _statusIcon = IconName.CircleCheck;
}
```

```xml
<icons:Icon Kind="{Binding StatusIcon}" />
```

## Names from strings

`IconCatalog.TryParse` accepts a kebab-case id, a PascalCase name or a Lucide alias, ignoring case:

```csharp
IconCatalog.TryParse("chevron-right", out var a);   // IconName.ChevronRight
IconCatalog.TryParse("ChevronRight", out var b);    // IconName.ChevronRight
IconCatalog.TryParse("home", out var c);            // IconName.House — Lucide renamed home → house
IconCatalog.TryParse("nope", out var d);            // false, IconName.None
```

A real icon id always wins over another icon's alias.

## Metadata and search

```csharp
IconInfo house = IconCatalog.Get(IconName.House);
// house.Id         "house"
// house.Source     "lucide" (or "custom")
// house.Tags       ["home", "living", "building", "residence", "architecture"]
// house.Categories ["buildings", "home"]
// house.Aliases    ["home"]

foreach (var icon in IconCatalog.Search("cart"))
    Console.WriteLine(icon.Id);   // shopping-cart, … then icons tagged "cart"
```

`IconCatalog.All` lists every icon; `IconCatalog.Count` is the catalog size. That is enough to build an icon picker:

```xml
<ItemsControl ItemsSource="{Binding Results}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" Spacing="8">
                <icons:Icon Kind="{Binding Name}" />
                <TextBlock Text="{Binding Id}" />
            </StackPanel>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```
