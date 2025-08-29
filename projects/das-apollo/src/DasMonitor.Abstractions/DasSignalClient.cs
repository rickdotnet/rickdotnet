using System.Net.Http.Json;

namespace DasMonitor.Abstractions;

public record DasResult(bool Success, string? ErrorMessage = null);

public class DasSignalClient
{
    private readonly HttpClient httpClient;

    public DasSignalClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        
        // Set the base address if it's not already set
        if (httpClient.BaseAddress == null)
            httpClient.BaseAddress = new Uri(DasMonitorConstants.BaseSignalUrl);
    }

    public async Task<DasResult> SendSignalAsync(AddSignalCommand command, CancellationToken cancellationToken)
    {
        var payload = new
        {
            Pid = command.ProductId,
            ZoneId = command.ZoneId,
            Color = command.HexColor,
            Effect = command.Effect.DirtyTransform(),
            Name = command.Title,
            Message = command.Message
        };

        var response = await httpClient.PostAsJsonAsync("", payload, cancellationToken);
        var result =
            new DasResult(
                response.IsSuccessStatusCode,
                ErrorMessage: response.IsSuccessStatusCode ? null : response.StatusCode.ToString()
            );

        return result;
    }

    public async Task<DasResult> ClearSignalAsync(ClearSignalCommand command, CancellationToken cancellationToken)
    {
        var url = $"pid/{command.ProductId}/zoneId/{command.ZoneId}";

        var response = await httpClient.DeleteAsync(url, cancellationToken);
        var result =
            new DasResult(
                response.IsSuccessStatusCode,
                ErrorMessage: response.IsSuccessStatusCode ? null : response.StatusCode.ToString()
            );

        return result;
    }
}