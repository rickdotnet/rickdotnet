using System.Net.Http.Json;
using DasMonitor.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DasMonitor;

/// <summary>
/// Das Test Cases
/// </summary>
/// <remarks>This is a junk class to test the DasMonitor</remarks>
public class DasDemo
{
    private const string BaseUrl = "http://localhost:27301/api/1.0/signals";
    private const string Red = "#F00";
    private const string White = "#FFF";
    private const string Key1Coordinate = "2,1";
    private const string Key1 = "KEY_1";
    private const string ProductId = "DK5QPID";

    public static async Task SimpleRun()
    {
        Console.WriteLine("Red: 1, White: 0, Delete: -1");
        var choice = Console.ReadLine();
        var delete = choice == "-1";
        var red = choice == "1";

        // Create the JSON payload  
        var payload = new
        {
            Pid = ProductId,
            ZoneId = Key1,
            Color = red ? Red : White,
            Effect = "BLINK",
            Name = "Status Update",
            Message = "Sent from DasConsole"
        };

        if (!delete)
            Console.WriteLine($"Sending signal to zoneId: {payload.ZoneId} at {BaseUrl}");

        // Send the POST request  
        try
        {
            using var httpClient = new HttpClient();
            if (delete)
            {
                var deleteResponse =
                    await httpClient.DeleteAsync($"{BaseUrl}/pid/{ProductId}/zoneId/{payload.ZoneId}");
                Console.WriteLine(deleteResponse.IsSuccessStatusCode ? "OK" : $"ERROR: {deleteResponse.StatusCode}");
            }
            else
            {
                var response = await httpClient.PostAsJsonAsync(BaseUrl, payload);
                Console.WriteLine(response.IsSuccessStatusCode ? "OK" : $"ERROR: {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response:\n{responseContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    public static async Task RedNumbers()
    {
        using var httpClient = new HttpClient();

        var tasks = Enumerable.Range(1, 9).Select(x =>
        {
            var payload = new
            {
                pid = ProductId,
                zoneId = $"KEY_{x}",
                color = Red,
                effect = "BLINK"
            };

            return httpClient.PostAsJsonAsync(BaseUrl, payload);
            //Console.WriteLine(response.IsSuccessStatusCode ? "OK" : $"ERROR: {response.StatusCode}");
        });

        await Task.WhenAll(tasks);
    }

    public static async Task ClearNumbers()
    {
        using var httpClient = new HttpClient();

        foreach (var x in Enumerable.Range(1, 9))
        {
            var payload = new
            {
                pid = ProductId,
                zoneId = $"KEY_{x}"
            };

            var response = await httpClient.DeleteAsync($"{BaseUrl}/pid/{ProductId}/zoneId/{payload.zoneId}");
            Console.WriteLine(response.IsSuccessStatusCode ? "OK" : $"ERROR: {response.StatusCode}");
        }
    }

    public static async Task RedOut()
    {
        using var httpClient = new HttpClient();
        foreach (var x in Enumerable.Range(1, 23))
        {
            foreach (var y in Enumerable.Range(1, 5))
            {
                var payload = new
                {
                    pid = ProductId,
                    zoneId = $"{x},{y}",
                    color = Red,
                    effect = "BLINK",
                };

                var response = await httpClient.PostAsJsonAsync(BaseUrl, payload);
                Console.WriteLine(response.IsSuccessStatusCode ? "OK" : $"ERROR: {response.StatusCode}");
            }
        }
    }

    public static async Task Clear()
    {
        using var httpClient = new HttpClient();
        foreach (var x in Enumerable.Range(1, 23))
        {
            foreach (var y in Enumerable.Range(1, 5))
            {
                var payload = new
                {
                    pid = ProductId,
                    zoneId = $"{x},{y}",
                };

                var response = await httpClient.DeleteAsync($"{BaseUrl}/pid/{ProductId}/zoneId/{payload.zoneId}");
                Console.WriteLine(response.IsSuccessStatusCode ? "OK" : $"ERROR: {response.StatusCode}");
            }
        }
    }

    public static async Task HostDemo(IHost host)
    {
        var t = host.RunAsync();
        using var scope = host.Services.CreateScope();
        var provider = scope.ServiceProvider;

        Console.WriteLine("Waiting for Apollo to start...");
        await Task.Delay(2000);
        var publisher = provider.GetRequiredService<DasPublisher>();
        await publisher.PublishSignal(
            new AddSignalCommand
            {
                ProductId = "DK5QPID",
                ZoneId = "KEY_1",
                HexColor = "#F00",
                Effect = DasEffect.Blink,
                Title = "Apollo Test",
                Message = "Hi from Apollo!"
            });

        await Task.Delay(1000);
        await publisher.ClearSignal(new ClearSignalCommand{ ProductId = "DK5QPID", ZoneId = "KEY_1" });
        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
    }
}