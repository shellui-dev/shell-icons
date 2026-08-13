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
    o.SiteTagline = "Lucide-derived SVG icons for Blazor.";
    o.GitHubRepo = "shellui-dev/shell-icons";

    // Sidebar-nav layout — top-nav links move into the sidebar's header.
    o.LayoutVariant = DocsLayoutVariant.Sidebar;

    o.AddNavLink("Docs", "/docs/introduction");
    o.AddNavLink("Icons", "/icons");
    o.AddNavLink("GitHub", "https://github.com/shellui-dev/shell-icons");

    // Every generated ShellIcons component is available inside razor:preview blocks.
    o.RegisterComponentsFromAssembly<global::ShellIcons.IconCore>();

    // Preview wrappers — razor:preview fences require a registered component as
    // the outer tag, and Markdig mangles inline HTML like <span> between component
    // slots. PreviewRow / PreviewCell survive both because they're registered too.
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
