using Microsoft.AspNetCore.Mvc;
using NATS.Client.KeyValueStore;
using RickDotNet.Extensions.Base;

namespace Vault.Web.Routing;

public static class Api
{
    public static void MapApi(this IEndpointRouteBuilder app)
    {
        app.MapKv();
    }

    private static void MapKv(this IEndpointRouteBuilder api)
    {
        var kv = api.MapGroup("/kv");
        // for now, we're going to create on first put
        kv.MapPut("{vault}", CreateOrUpdateVault).RequireAuthorization("vault.admin");
        kv.MapDelete("{vault}", DeleteVault).RequireAuthorization("vault.admin");
        kv.MapGet("{vault}/{key}", GetKey).RequireAuthorization("vault.read");
        kv.MapPut("{vault}/{key}", PutKey).RequireAuthorization("vault.write");
        kv.MapDelete("{vault}/{key}", DeleteKey).RequireAuthorization("vault.write"); // could restrict to admin + key writer
    }

    private static async Task<IResult> CreateOrUpdateVault(
        string vault,
        [FromBody] VaultSlip request,
        [FromServices] VaultSettings vaultSettings,
        [FromServices] INatsKVContext kv,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userKey = httpContext.User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userKey))
            return Results.Unauthorized();
        
        var signer = httpContext.User.Claims.FirstOrDefault(c => c.Type == JwtUtil.SignerClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(signer))
            return Results.Unauthorized();
        
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            request = request with { DisplayName = vault };

        var actualVault = await kv.GetVault(vault, cancellationToken).ValueOrDefaultAsync();
        if (actualVault is null)
        {
            // first PUT to a vault will be done by a vault issued admin-token
            var isVaultSigned = signer.Equals(vaultSettings.IssuerKey, StringComparison.OrdinalIgnoreCase);
            if (!isVaultSigned)
                return Results.Unauthorized(); // until we have things on lock, let's not leak 404s

            var vaultSlip = request with
            {
                VaultId = vault,
                OwnerKey = string.IsNullOrWhiteSpace(request.OwnerKey) 
                    ? userKey 
                    : request.OwnerKey,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            actualVault = await kv.CreateVault(vaultSlip, cancellationToken).ValueOrDefaultAsync();
            return actualVault is null 
                ? Results.InternalServerError() 
                : Results.Ok(actualVault.VaultSlip);
        }

        var updatedSlip = new VaultSlip
        {
            VaultId = vault,
            OwnerKey = request.OwnerKey,
            DisplayName = request.DisplayName,
            Description = request.Description,
            Signers = request.Signers,
            Expires = request.Expires,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        
        var returnedSlip = await kv.UpdateVault(updatedSlip, cancellationToken: cancellationToken).ValueOrDefaultAsync();

        // TODO: if this user owns the vault
        //       give them a better error message
        //       until  we have a better error message
        //       don't leak the vault
        return returnedSlip is not null
            ? Results.Ok(returnedSlip)
            : Results.Unauthorized();
    }

    private static async Task<IResult> DeleteVault(
        string vault,
        [FromServices] INatsKVContext kv,
        [FromHeader(Name = "Authorization")] string authHeader,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(authHeader))
            return Results.BadRequest();

        var vaultUser = VaultUser.FromAuthHeader(authHeader).ValueOrDefault();
        if (vaultUser is null)
            return Results.Unauthorized();

        var hasAdmin = vaultUser.HasVaultAdmin(vault);
        if (!hasAdmin)
            return Results.Unauthorized();

        var actualVault = await kv.GetVault(vault, cancellationToken).ValueOrDefaultAsync();
        if (actualVault is null)
            return Results.Unauthorized();

        var validSigner = await actualVault.ValidateSigner(vaultUser.SignerKey, cancellationToken);
        if (!validSigner)
            return Results.Unauthorized();

        var result = await actualVault.DeleteVault(cancellationToken);
        return result.Successful
            ? Results.StatusCode(204)
            : Results.BadRequest();
    }

    private static async Task<IResult> GetKey(
        string vault,
        string key,
        [FromServices] INatsKVContext kvContext,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var actualVault = await kvContext.GetVault(vault, cancellationToken).ValueOrDefaultAsync();
        if (actualVault is null)
            return Results.Unauthorized(); // until we have things on lock, let's not leak vaults

        var signer = httpContext.User.Claims.FirstOrDefault(c => c.Type == JwtUtil.SignerClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(signer))
            return Results.Unauthorized();

        var validSigner = await actualVault.ValidateSigner(signer, cancellationToken);
        if (!validSigner)
            return Results.Unauthorized();

        var result = await actualVault.GetValue(key, cancellationToken);
        if (result.NotSuccessful)
            return Results.NotFound();

        // we're only supporting bytes for now
        // serialize on your own time, buddy
        var bytes = result.ValueOrDefault()!;

        // TODO: inspect the accept header and return the appropriate content type
        return Results.Bytes(bytes);
    }

    private static async Task<IResult> PutKey(
        string vault,
        string key,
        HttpContext httpContext,
        [FromServices] INatsKVContext kvContext,
        CancellationToken cancellationToken)
    {
        var actualVault = await kvContext.GetVault(vault, cancellationToken).ValueOrDefaultAsync();
        if (actualVault is null)
            return Results.Unauthorized(); // until we have things on lock, let's not leak vaults

        var signer = httpContext.User.Claims.FirstOrDefault(c => c.Type == JwtUtil.SignerClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(signer))
            return Results.Unauthorized();

        var validSigner = await actualVault.ValidateSigner(signer, cancellationToken);
        if (!validSigner)
            return Results.Unauthorized();

        byte[] bytes;
        using (var memoryStream = new MemoryStream())
        {
            await httpContext.Request.Body.CopyToAsync(memoryStream, cancellationToken);
            bytes = memoryStream.ToArray();
        }

        var result = await actualVault.PutValue(key, bytes, cancellationToken);
        return result.Successful
            ? Results.Ok(new { Revision = result.ValueOrDefault() })
            : Results.BadRequest();
    }

    private static async Task<IResult> DeleteKey(
        string vault,
        string key,
        HttpContext httpContext,
        [FromServices] INatsKVContext kvContext,
        CancellationToken cancellationToken)
    {
        var actualVault = await kvContext.GetVault(vault, cancellationToken).ValueOrDefaultAsync();
        if (actualVault is null)
            return Results.Unauthorized(); // until we have things on lock, let's not leak vaults

        var signer = httpContext.User.Claims.FirstOrDefault(c => c.Type == JwtUtil.SignerClaimType)?.Value;
        if (string.IsNullOrWhiteSpace(signer))
            return Results.Unauthorized();

        var validSigner = await actualVault.ValidateSigner(signer, cancellationToken);
        if (!validSigner)
            return Results.Unauthorized();

        var result = await actualVault.DeleteValue(key, cancellationToken);

        return result.Successful
            ? Results.StatusCode(204)
            : Results.BadRequest();
    }
}
