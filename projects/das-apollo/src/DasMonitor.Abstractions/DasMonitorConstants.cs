using Apollo.Configuration;

namespace DasMonitor.Abstractions;

public static class DasMonitorConstants
{
    public const string BaseSignalUrl = "http://localhost:27301/api/1.0/signals/";
    public static readonly EndpointConfig EndpointConfig = new()
    {
        EndpointName = "daskeyboard Signal Monitor",
        Subject = "daskeyboard.signals",
        ConsumerName = "das-consumer"
    };

    public static readonly PublishConfig PublishConfig = EndpointConfig.ToPublishConfig();
    
    public static string DirtyTransform(this DasEffect effect)
    {
        return effect switch
        {
            DasEffect.SetColor => "SET_COLOR",
            DasEffect.ColorCycle => "COLOR_CYCLE",
            _ => effect.ToString().ToUpper()
        };
    }
}