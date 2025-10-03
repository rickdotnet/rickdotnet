using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using NATS.NKeys;
using RickDotNet.Base.Utils;
using RickDotNet.Extensions.Base;
using StarFederation.Datastar.DependencyInjection;
using Vault.Web.Components.Fragments;
using Vault.Web.Components.Parts;
using Vault.Web.Models;

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
        demo.MapPost("console", HandleConsole).RequireAuthorization();
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
            var vaultId = "abc123";

            var vaultPair = KeyPair.FromSeed("SAAALU7ARSDQVTULFDHD4MGHRCM2AWAZ4HAVWQTH5RSWUUPEB2ORJXDZRY");
            var userPair = KeyPair.CreatePair(PrefixByte.Account);
            var userPublic = userPair.GetPublicKey();
            var adminClaims = new VaultPermissionsJwt
            {
                Subject = userPair.GetPublicKey(), // could be different from the signer
                Expires = DateTimeOffset.UtcNow.AddMonths(6),
                Permissions =
                [
                    $"vault:admin:{vaultId}",
                ]
            };
            token = JwtUtil.Encode(adminClaims, vaultPair);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userPublic),
                new Claim("default-vault", vaultId),
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
            
            Dictionary<string, object?> userInfoParams = [];
            userInfoParams["UserInfo"] = new UserInfo(userPublic, vaultId);

            var html = await htmlRenderer.RenderHtmlAsync<ConsoleInput>(userInfoParams);
            await datastarService.PatchElementsAsync(html, cancellationToken);
            
            html = await htmlRenderer.RenderHtmlAsync<TopNav>(userInfoParams);
            await datastarService.PatchElementsAsync(html, cancellationToken);
            
            Dictionary<string, object?> vaultParams = [];
            vaultParams["VaultId"] = vaultId;
            html = await htmlRenderer.RenderHtmlAsync<VaultScreen>(vaultParams);
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
        var user = ctx.User;
        var htmlRenderer = ctx.RequestServices.GetRequiredService<HtmlRenderer>();
        var datastarService = ctx.RequestServices.GetRequiredService<IDatastarService>();

        UserInfo? userInfo = string.IsNullOrEmpty(user.Identity?.Name)
            ? null
            : new(
                user.Identity.Name,
                user.Claims.FirstOrDefault(c => c.Type == "default-vault")?.Value
            );

        Dictionary<string, object?> userInfoParams = [];
        userInfoParams["UserInfo"] = userInfo;

        // handle command and stream result back as necessary
    }

    private static async Task HandleVault(
        HttpContext ctx,
        CancellationToken cancellationToken)
    {
        // get vault id from url
        // bust out aether for vault events
        
    }
    private static async Task<IResult> HandlePage(
        HttpContext ctx,
        CancellationToken cancellationToken)
    {

        // could return datastar divs and have them subsequently
        // hit their own respective routes
        var fullHtml = $@"
            <div id=""welcome"">
                
            </div>
            <div id=""console-input"">
               
            </div>
        ";

        return Results.Content(fullHtml, "text/html");
    }
}
