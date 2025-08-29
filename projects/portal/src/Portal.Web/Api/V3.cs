using Portal.Web.Api.Client;

namespace Portal.Web.Api;

public static class V3
{
    public static IEndpointRouteBuilder MapV3(this IEndpointRouteBuilder api)
    {
        api.MapClient();
        
        return api;
    }
    
    private static void MapClient(this IEndpointRouteBuilder api)
    {
        var v3 = api.MapGroup("/_matrix/client/v3");
        
        v3.MapRegisterApi();
    }
    
}
