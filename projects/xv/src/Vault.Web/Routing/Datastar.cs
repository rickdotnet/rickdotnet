using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NATS.NKeys;
using StarFederation.Datastar;
using StarFederation.Datastar.DependencyInjection;
using Vault.Web.Components.Layout;
using Vault.Web.Components.Parts;
using Vault.Web.Models;
using Index = Vault.Web.Components.Pages.Index;

namespace Vault.Web.Routing;

public static class Datastar
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
            userInfoParams["IndexSignals"] = new IndexSignals
            {
                PublicKey = userPublic,
                DefaultVault = vaultId
            };


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
        // if command == login, then handle login
        // if success, render index with div pointing to SSE endpoint
        // if failure, update error message signal 

        // all other commands require a logged in user
        var user = ctx.User;

        // if they are not logged in, set error message signal
        // and flip the submitting flag

        // if they are logged in
        // run command and patch front-end

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
        var html = await htmlRenderer.RenderHtmlAsync<ConsoleInput>(userInfoParams);
        await datastarService.PatchElementsAsync(html, new PatchElementsOptions
        {
            Selector = null,
            PatchMode = ElementPatchMode.Outer,
            UseViewTransition = false,
            EventId = null,
            Retry = default
        }, cancellationToken);
    }

    private static async Task HandleVault(
        HttpContext ctx,
        CancellationToken cancellationToken)
    {
        // get vault id from url
        // bust out aether for vault events
    }

    
}
