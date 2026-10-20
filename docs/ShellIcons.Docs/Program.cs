using ShellIcons.Docs.Components;
using ShellDocs.Components;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddShellDocs(o =>
{
    o.ContentRoot = Path.Combine(builder.Environment.ContentRootPath, "content");
    o.SiteName = "ShellIcons";
    o.SiteTagline = "Lucide-derived icons for Blazor, .NET MAUI and Avalonia.";
    o.GitHubRepo = "shellui-dev/shell-icons";
    o.SiteUrl = "https://shellicons.shellui.dev";
    o.LayoutVariant = DocsLayoutVariant.Sidebar;
    o.SetLogo("/images/logo-light.svg", "/images/logo-dark.svg", "ShellIcons");

    // /llms.txt, every page as Markdown, agent instruction files and the page actions menu.
    o.EnableLlmsTxt(l =>
    {
        l.Summary = "ShellIcons is the Lucide icon set for .NET: typed components for Blazor (inline SVG), and native controls for .NET MAUI and Avalonia, generated from one catalog at build time.";
        l.RulesFile = "Llms/agent-rules.md";
        l.InstallCommandPattern = @"dotnet add package ShellIcons\.\w+(?: --version \S+)?";
    });

    o.AddNavLink("Docs", "/docs/introduction");
    o.AddNavLink("Icons", "/docs/icons");
    o.AddNavLink("AI Agents", "/docs/ai-agents");
    o.AddNavLink("GitHub", "https://github.com/shellui-dev/shell-icons");

    // Every icon component, usable in razor:preview blocks.
    o.RegisterComponentsFromAssembly<global::ShellIcons.IconCore>();

    // This site's own components: PreviewRow/PreviewCell layout helpers and IconBrowser (content/docs/icons.md).
    o.RegisterComponentsFromAssembly<ShellIcons.Docs.Preview.PreviewRow>();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
