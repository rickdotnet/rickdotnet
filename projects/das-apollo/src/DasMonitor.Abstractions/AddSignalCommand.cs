using Apollo.Abstractions;

namespace DasMonitor.Abstractions;

public record AddSignalCommand : ICommand
{
    /// <summary>
    /// Pid of the device targeted by the signal
    /// </summary>
    /// <example>DK5QPID</example>
    public required string ProductId { get; init; }

    /// <summary>
    /// Zone id of the key targeted by the signal
    /// </summary>
    /// <remarks>https://www.daskeyboard.io/q-zone-id-explanation</remarks>
    public required string ZoneId { get; init; }

    /// <summary>
    /// Three or six character hex color code.
    /// </summary>
    public string? HexColor { get; set; }

    /// <summary>
    /// DasEffect to apply to the signal
    /// </summary>
    public DasEffect Effect { get; set; } = DasEffect.None;
    
    /// <summary>
    /// Notification title
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Notification message
    /// </summary>
    public string? Message { get; set; }
}

public enum DasEffect
{
    None,
    SetColor,
    Blink,
    Breathe,
    ColorCycle
}