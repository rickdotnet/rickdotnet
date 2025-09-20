namespace Vault;

public record VaultSettings
{
    public string NatsUrl { get; init; } = "nats://rhino-nats.cloud:4222";
    public string IssuerKey { get; init; } = "ADDFAP4ENPWIVIB6VFEHIX4LQBGJN2LANFN24B4DU6IXTGYV7O6ZVWUL";
    //public string IssuerSeed { get; init; } = "SAAALU7ARSDQVTULFDHD4MGHRCM2AWAZ4HAVWQTH5RSWUUPEB2ORJXDZRY";
    public string? NatsUser { get; init; } = "vault";
    public string? NatsPass { get; init; } = "ff6bb3b1017b4cbeb0a3d5ef8bdef45c";
}