namespace Vault.Web;

public static class FragmentPaths
{
    public const string Help = "@get('/fragments/help')";
    public static class Login
    {
        public const string GetToken = "@get('/fragments/login/token')";
        public const string GetSeed = "@get('/fragments/login/seed')";
        public const string GetGenerate = "@get('/fragments/login/generate')";
    }
}
