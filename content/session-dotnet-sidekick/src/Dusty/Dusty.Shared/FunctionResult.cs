using System.ComponentModel;

namespace Dusty.Shared;

[Description("Represents the result of a function execution, indicating success or error.")]
public record FunctionResult(bool IsSuccess, string? ErrorMessage = null)
{
    public static readonly FunctionResult Success = new(true);
    public static FunctionResult Error(string error) => new(false, error);
}
