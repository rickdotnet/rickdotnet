
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vault.Web.Components.Layout;
using Vault.Web.Models;

namespace Vault.Web.Routing;

public static class App
{
    public static void MapApp(this IEndpointRouteBuilder app)
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
        
         var fullHtml = await htmlRenderer.RenderHtmlAsync<MainLayout>();
         return Results.Content(fullHtml, "text/html");
    }
}
