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
    o.SiteTagline = "Lucide-derived icons for Blazor and .NET MAUI.";
    o.GitHubRepo = "shellui-dev/shell-icons";
    o.LayoutVariant = DocsLayoutVariant.Sidebar;

    o.AddNavLink("Docs", "/docs/introduction");
    o.AddNavLink("Icons", "/docs/icons");
    o.AddNavLink("GitHub", "https://github.com/shellui-dev/shell-icons");

    // Every icon component, usable in razor:preview blocks.
    o.RegisterComponentsFromAssembly<global::ShellIcons.IconCore>();

    /* This site's own components: PreviewRow/PreviewCell (razor:preview needs a registered outer tag,
       and Markdig mangles plain <span> wrappers) and IconBrowser (used from content/docs/icons.md). */
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
