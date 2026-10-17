using Avalonia.Metadata;

// One xmlns for the dispatcher and the typed controls; the prefix keeps names like Image apart from Avalonia's.
[assembly: XmlnsDefinition("https://shellicons.dev/avalonia", "ShellIcons.Avalonia")]
[assembly: XmlnsDefinition("https://shellicons.dev/avalonia", "ShellIcons.Avalonia.Icons")]
[assembly: XmlnsPrefix("https://shellicons.dev/avalonia", "icons")]
