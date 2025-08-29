using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Portal.Web.Api.Client;

public static class Register
{
    public static IEndpointRouteBuilder MapRegisterApi(this IEndpointRouteBuilder api)
    {
        api.MapPost("/register", Post);

        return api;
    }

    private static async Task<IResult> Post(
        [FromBody] RegisterRequest request,
        IUserStore store,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request!.Username))
        {
            return Results.Json(
                new MatrixError
                {
                    ErrCode = "M_INVALID_PARAM",
                    Error = "Username is required"
                },
                statusCode: 400
            );
        }

        if (await store.UserExistsAsync(request.Username))
        {
            return Results.Json(
                new MatrixError
                {
                    ErrCode = "M_USER_IN_USE",
                    Error = "Username is already taken"
                },
                statusCode: 400
            );
        }

        var userId = $"@{request.Username}:localhost";
        var accessToken = await store.RegisterUserAsync(userId, request.Password);

        return Results.Ok(
            new RegisterResponse
            {
                UserId = userId,
                AccessToken = accessToken,
                DeviceId = "DEVICE123"
            }
        );
    }

    private static async Task<IResult> InternalPost(HttpContext context)
    {
        var request = await context.Request.ReadFromJsonAsync<RegisterRequest>();
        var store = context.RequestServices.GetRequiredService<IUserStore>();
        if (string.IsNullOrEmpty(request!.Username))
        {
            return Results.Json(
                new MatrixError
                {
                    ErrCode = "M_INVALID_PARAM",
                    Error = "Username is required"
                },
                statusCode: 400
            );
        }

        if (await store.UserExistsAsync(request.Username))
        {
            return Results.Json(
                new MatrixError
                {
                    ErrCode = "M_USER_IN_USE",
                    Error = "Username is already taken"
                },
                statusCode: 400
            );
        }

        var userId = $"@{request.Username}:localhost";
        var accessToken = await store.RegisterUserAsync(userId, request.Password);

        return Results.Ok(
            new RegisterResponse
            {
                UserId = userId,
                AccessToken = accessToken,
                DeviceId = "DEVICE123"
            }
        );
    }
}
public record RegisterRequest
{
    [Required]
    public required string Username { get; init; }

    public required string Password { get; init; }
    public string Kind { get; init; } = "user";
}

public record RegisterResponse
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("device_id")]
    public required string DeviceId { get; init; }
}

public record MatrixError
{
    [JsonPropertyName("errcode")]
    public required string ErrCode { get; init; }

    [JsonPropertyName("error")]
    public required string Error { get; init; }
}

public interface IUserStore
{
    Task<bool> UserExistsAsync(string username);

    Task<string> RegisterUserAsync(string userId, string password);
}

public class InMemoryUserStore : IUserStore
{
    private readonly Dictionary<string, User> users = new();

    public async Task<bool> UserExistsAsync(string username)
    {
        return await Task.FromResult(users.ContainsKey($"@{username}:localhost"));
    }

    public async Task<string> RegisterUserAsync(string userId, string password)
    {
        var accessToken = Guid.NewGuid().ToString();
        users[userId] = new User { UserId = userId, Password = password, AccessToken = accessToken };
        return await Task.FromResult(accessToken);
    }

    private record User
    {
        public required string UserId { get; init; }
        public required string Password { get; init; }
        public required string AccessToken { get; init; }
    }
}
