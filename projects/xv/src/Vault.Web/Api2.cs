using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using RickDotNet.Base.Utils;
using RickDotNet.Extensions.Base;
using StarFederation.Datastar.DependencyInjection;
using Vault.Web.Components.Fragments;
using Vault.Web.Components.Parts;

namespace Vault.Web;

public static class Api2
{
    public static void MapDataStar(this IEndpointRouteBuilder app)
    {
        app.MapSSE();
    }

    private static void MapSSE(this IEndpointRouteBuilder api)
    {
        var demo = api.MapGroup("/sse");
        demo.MapPost("login", HandleLogin);
        demo.MapPost("console", HandleConsole);
    }

    private static async Task HandleLogin(
        HttpContext ctx,
        CancellationToken cancellationToken)
    {
        var contentType = ctx.Request.ContentType;
        var htmlRenderer = ctx.RequestServices.GetRequiredService<HtmlRenderer>();
        var datastarService = ctx.RequestServices.GetRequiredService<IDatastarService>();

        if (contentType == "application/x-www-form-urlencoded")
        {
            var form = await ctx.Request.ReadFormAsync(cancellationToken);
            var seed = form["seed"].ToString();
            var token = form["token"].ToString();
            var generate = form["generate"].ToString();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "test"), 
                new Claim(ClaimTypes.Role, "User"), 
            };

           
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme 
            );
            
            var principal = new ClaimsPrincipal(identity);
            
            await ctx.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true, // Persist the cookie across sessions
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1) // Set expiration
                });
            
            

            var html = await htmlRenderer.RenderHtmlAsync<ConsoleInput>();
            
            await datastarService.PatchElementsAsync(html, cancellationToken);

            html = await htmlRenderer.RenderHtmlAsync<TopNav>();
            await datastarService.PatchElementsAsync(html, cancellationToken);
        }
        else
        {
            var returnHtml =
                """
                <div id="console-response">Bad Response</div>
                """;

            await datastarService.PatchElementsAsync(returnHtml, cancellationToken);
        }
    }

    private static async Task HandleConsole(
        HttpContext ctx,
        CancellationToken cancellationToken)
    {
        var htmlRenderer = ctx.RequestServices.GetRequiredService<HtmlRenderer>();
        var html = await htmlRenderer.RenderHtmlAsync<ConsoleInput>();
        
        var datastarService = ctx.RequestServices.GetRequiredService<IDatastarService>();
        await datastarService.PatchElementsAsync(html, cancellationToken);

        html = await htmlRenderer.RenderHtmlAsync<TopNav>();
        await datastarService.PatchElementsAsync(html, cancellationToken);


    }

    private static async Task<IResult> HandleDemoAction(
        HttpContext ctx,
        CancellationToken cancellationToken)
    {
        var htmlRenderer = ctx.RequestServices.GetRequiredService<HtmlRenderer>();

        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { "Message", "Rendered on the server!" }
        });

        var html = await htmlRenderer.RenderHtmlAsync<Login>(parameters);

        if (!ctx.Request.ContentType?.Contains("application/x-www-form-urlencoded") == true &&
            !ctx.Request.ContentType?.Contains("multipart/form-data") == true)
        {
            return Results.BadRequest("Expected form data");
        }

        var form = await ctx.Request.ReadFormAsync(cancellationToken);
        var seed = form["seed"].ToString();
        var token = form["token"].ToString();
        var generate = form["generate"].ToString();

        string responseHtml;
        string consoleInputHtml;

        if (!string.IsNullOrEmpty(generate) && generate == "true")
        {
            // Handle generate action - stub implementation
            responseHtml = "<div id='welcome-block'>Generate action processed</div>";
            consoleInputHtml = "<div id='console-input'>Generate command executed</div>";
        }
        else if (!string.IsNullOrEmpty(seed))
        {
            // Handle seed action - stub implementation
            responseHtml = $"<div id='welcome-block'>Seed action processed: {seed}</div>";
            consoleInputHtml = $"<div id='console-input'>Seed: {seed}</div>";
        }
        else if (!string.IsNullOrEmpty(token))
        {
            // Handle token action - stub implementation
            responseHtml = $"<div id='welcome-block'>Token action processed: {token}</div>";
            consoleInputHtml = $"<div id='console-input'>Token: {token}</div>";
        }
        else
        {
            return Results.BadRequest("Missing seed, token, or generate parameter");
        }

        // Return HTML that will replace the welcome block and console input block
        var fullHtml = $@"
            <div id=""welcome-block"">
                {responseHtml}
            </div>
            <div id=""console-input-block"">
                {consoleInputHtml}
            </div>
        ";

        return Results.Content(fullHtml, "text/html");
    }
}
