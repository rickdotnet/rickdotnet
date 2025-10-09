
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vault.Web.Components.Layout;
using Vault.Web.Models;

namespace Vault.Web.Routing;

public static class Pages
{
    public static void MapPages(this IEndpointRouteBuilder app)
    {
        app.MapIndex();
    }
    
    private static void MapIndex(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", HandleIndex);
    }
    
    private static async Task<IResult> HandleIndex(
        HttpContext ctx,
        CancellationToken cancellationToken)
    {
        var htmlRenderer = ctx.RequestServices.GetRequiredService<HtmlRenderer>();
        // var datastarService = ctx.RequestServices.GetRequiredService<IDatastarService>();

        var indexSignals = new IndexSignals
        {
            PublicKey = "abc123",
            DefaultVault = "abc123"
        };
        
        
        RenderFragment indexFragment = builder =>
        {
            builder.OpenComponent<Vault.Web.Components.Pages.Index>(0);
            builder.AddAttribute(1, "IndexSignals", indexSignals);
            builder.CloseComponent();
        };

        var layoutParams = new Dictionary<string, object?>
        {
            ["Body"] = indexFragment
        };

        var fullHtml = await htmlRenderer.RenderHtmlAsync<MainLayout>(layoutParams);
        return Results.Content(fullHtml, "text/html");
    }
}
