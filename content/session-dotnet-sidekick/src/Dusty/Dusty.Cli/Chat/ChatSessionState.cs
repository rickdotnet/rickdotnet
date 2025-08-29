using Microsoft.Extensions.AI;

namespace Dusty.Cli.Chat;

public record ChatSessionState
{
    public ChatMode Mode { get; set; } = ChatMode.Chat;
    public List<ChatMessage> Messages { get; set; } = [];
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    
    public ChatSessionState(string systemMessage)
    {
        Messages.Add(new ChatMessage(ChatRole.System, systemMessage));
    }
    
    public void SetMode(ChatMode mode) => Mode = mode;

    public void AddMessage(ChatMessage message) => Messages.Add(message);
}