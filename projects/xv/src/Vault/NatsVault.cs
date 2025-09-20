using NATS.Client.KeyValueStore;
using RickDotNet.Base;
using RickDotNet.Extensions.Base;

namespace Vault;

public class NatsVault
{
    public VaultSlip VaultSlip { get; }
    private readonly INatsKVContext kvContext;
    private INatsKVStore? kvStore;

    public const string KvPrefix = "vault_";

    public NatsVault(INatsKVContext context, VaultSlip vaultSlip)
    {
        this.VaultSlip = vaultSlip;
        kvContext = context;
    }

    public Task<Result<INatsKVStore>> CreateStore(CancellationToken cancellationToken = default)
    {
        if (kvStore != null)
            return Result.ErrorTask<INatsKVStore>("Store already exists");

        return Result.TryAsync(async () =>
        {
            var config = VaultSlip.KvConfig();
            kvStore = await kvContext.CreateStoreAsync(config, cancellationToken);
            return kvStore;
        });
    }

    public Task<Result<INatsKVStore>> CreateOrUpdateStore()
    {
        return Result.TryAsync(async () =>
        {
            var config = VaultSlip.KvConfig();
            kvStore = await kvContext.CreateOrUpdateStoreAsync(config);
            return kvStore;
        });
    }

    public Task<Result<INatsKVStore>> UpdateStore(CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(async () =>
        {
            var config = VaultSlip.KvConfig();
            kvStore = await kvContext.UpdateStoreAsync(config, cancellationToken);
            return kvStore;
        });
    }

    public Task<Result<byte[]>> GetValue(string key, CancellationToken cancellationToken = default)
    {
        return GetStore().BindAsync(async store =>
        {
            var entry = await store.GetEntryAsync<byte[]>(key, cancellationToken: cancellationToken);
            return entry.Value ?? Result.Error<byte[]>("Key not found");
        });
    }

    private Task<Result<VaultSlip>> GetVaultSlipFromMeta()
    {
        return GetStore().BindAsync(async store =>
        {
            var status = await store.GetStatusAsync();
            var metadata = status.Info.Config.Metadata?.ToDictionary() ?? new Dictionary<string, string>();
            var slip = new VaultSlip
            {
                VaultId = store.Bucket,
                OwnerKey = metadata.GetValueOrDefault("ownerKey") ?? throw new InvalidOperationException("Owner key is missing"),
                DisplayName = metadata.GetValueOrDefault("displayName") ?? store.Bucket,
                Description = metadata.GetValueOrDefault("description"),
                Signers = metadata.GetValueOrDefault("signers")?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [],
                Expires = metadata.TryGetValue("expires", out var expires) && DateTimeOffset.TryParse(expires, out var expiresDate)
                    ? expiresDate
                    : null,
                CreatedAt = metadata.TryGetValue("createdAt", out var createdAt) && DateTimeOffset.TryParse(createdAt, out var createdAtDate)
                    ? createdAtDate
                    : DateTimeOffset.UtcNow,
                UpdatedAt = metadata.TryGetValue("updatedAt", out var updatedAt) && DateTimeOffset.TryParse(updatedAt, out var updatedAtDate)
                    ? updatedAtDate
                    : null
            };
            
            return Result.Success(slip);

        });
    }


    public async Task<bool> VaultExists(CancellationToken cancellationToken = default)
    {
        return (await GetStore()).Successful;
    }

    private Task<Result<INatsKVStore>> GetStore()
    {
        if (kvStore != null)
            return Result.SuccessTask(kvStore);

        return Result.TryAsync(async () =>
        {
            var config = VaultSlip.KvConfig();
            kvStore = await kvContext.GetStoreAsync(config.Bucket);

            return kvStore;
        });
    }

    public Task<Result<Unit>> DeleteVault(CancellationToken cancellationToken = default)
    {
        return Result.TryAsync(async () =>
        {
            var config = VaultSlip.KvConfig();
            var success = await kvContext.DeleteStoreAsync(config.Bucket, cancellationToken);
            return success ? Result.Success() : Result.Error("Failed to delete store");
        });
    }

    public Task<Result<Unit>> ValidateSigner(string signer, CancellationToken cancellationToken = default)
    {
        return string.IsNullOrWhiteSpace(signer)
            ? Result.ErrorTask<Unit>("Signer claim is missing")
            : GetStore().BindAsync(async store =>
            {
                var valid = await store.IsValidSigner(signer, cancellationToken);
                return valid ? Result.Success() : Result.Error<Unit>("Unauthorized signer");
            });
    }

    /// <summary>
    /// Put a value into the vault under the specified key.
    /// </summary>
    /// <returns>Success with the revision number of the store if the value was put successfully, or an error if not.</returns>
    public Task<Result<ulong>> PutValue(string key, byte[] bytes, CancellationToken cancellationToken)
    {
        return GetStore()
            .SelectAsync(async store => await store.PutAsync(key, bytes, cancellationToken: cancellationToken));
    }

    public Task<Result<Unit>> DeleteValue(string key, CancellationToken cancellationToken)
    {
        return GetStore()
            .BindAsync(async store =>
            {
                await store.DeleteAsync(key, cancellationToken: cancellationToken);
                return Result.Success();
            });
    }
}

public static class NatsVaultExtensions
{
    public static async Task<bool> IsValidSigner(this INatsKVStore store, string signerKey, CancellationToken cancellationToken = default)
    {
        return await Result.TryAsync(() => store.GetStatusAsync(cancellationToken).AsTask())
            .BindAsync(status =>
            {
                var metadata = status.Info.Config.Metadata ?? new Dictionary<string, string>();
                var isOwner = metadata.TryGetValue("ownerKey", out var ownerKey) && ownerKey == signerKey;
                if (isOwner)
                    return Result.SuccessTask();

                if (!metadata.TryGetValue("signers", out var signers))
                    return Result.ErrorTask("Unauthorized signer");

                var signersList = signers.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (signersList.Contains(signerKey))
                    return Result.SuccessTask();

                return Result.ErrorTask<Unit>("Unauthorized signer");
            });
    }

    public static NatsKVConfig KvConfig(this VaultSlip vaultSlip, string prefix = NatsVault.KvPrefix)
    {
        var config = new NatsKVConfig($"{prefix}{vaultSlip.VaultId}")
        {
            Metadata = new Dictionary<string, string>()
            {
                { "ownerKey", vaultSlip.OwnerKey },
                { "displayName", vaultSlip.DisplayName },
                { "createdAt", (vaultSlip.CreatedAt ?? DateTime.UtcNow).ToString("o") }
            }
        };

        if (!string.IsNullOrWhiteSpace(vaultSlip.Description))
            config = config with { Description = vaultSlip.Description };

        if (vaultSlip.Signers?.Length > 0)
            config.Metadata["signers"] = string.Join(',', vaultSlip.Signers);

        if (vaultSlip.Expires.HasValue)
            config.Metadata["expires"] = vaultSlip.Expires.Value.ToString("o");

        if (vaultSlip.UpdatedAt.HasValue)
            config.Metadata["updatedAt"] = vaultSlip.UpdatedAt.Value.ToString("o");

        // metadata, ttl, etc
        return config;
    }

    /// <summary>
    /// Create a new vault with the given VaultSlip.
    /// </summary>
    /// <returns>Success if the vault was created, or an error, if not.</returns>
    public static Task<Result<NatsVault>> CreateVault(this INatsKVContext kvContext, VaultSlip vaultSlip, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultSlip.VaultId))
            return Result.ErrorTask<NatsVault>("Vault ID cannot be empty");

        var vault = new NatsVault(kvContext, vaultSlip);
        return vault.CreateStore(cancellationToken)
            .BindAsync(_ => Result.SuccessTask(vault));
    }

    public static Task<Result<VaultSlip>> UpdateVault(this INatsKVContext kvContext, VaultSlip vaultSlip, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(vaultSlip.VaultId))
            return Result.ErrorTask<VaultSlip>("Vault Id cannot be empty");

        var vault = new NatsVault(kvContext, vaultSlip);
        return vault.UpdateStore(cancellationToken)
            .BindAsync(_ => Result.SuccessTask(vault.VaultSlip));
    }


    public static Task<Result<NatsVault>> GetVault(this INatsKVContext kvContext, string vaultId, CancellationToken cancellationToken = default)
    {
        var vaultSlip = new VaultSlip { VaultId = vaultId };
        return kvContext.GetVault(vaultSlip, cancellationToken);
    }

    public async static Task<Result<NatsVault>> GetVault(this INatsKVContext kvContext, VaultSlip vaultSlip, CancellationToken cancellationToken = default)
    {
        var vault = new NatsVault(kvContext, vaultSlip);
        if (await vault.VaultExists(cancellationToken))
            return vault;

        return Result.Error<NatsVault>("Vault does not exist");
    }


    // public static Task<Result<INatsKVStore>> GetVaultStore(this INatsClient client, string vaultId)
    // {
    //     var vaultSlip = new VaultSlip { VaultId = vaultId };
    //     var vault = new NatsVault(client, vaultSlip);
    //     return vault.GetStore();
    // }
}
