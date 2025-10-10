using Microsoft.AspNetCore.Components.Web;
using Vault.Web.Components.Layouts;

namespace Vault.Web.Routing;

public static class App
{
    public static void MapApp(this IEndpointRouteBuilder app)
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
