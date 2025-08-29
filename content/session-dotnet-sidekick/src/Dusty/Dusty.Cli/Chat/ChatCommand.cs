namespace Dusty.Cli.Chat;

public record ChatCommand
{
    public CommandType CommandType { get; init; } = CommandType.Undefined;
    
    public string? Text { get; init; }
}
