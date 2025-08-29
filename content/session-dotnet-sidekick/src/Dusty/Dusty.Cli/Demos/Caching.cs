using Microsoft.Extensions.AI;

namespace Dusty.Cli.Demos;

public class Caching
{
    public static async Task Run()
    {
        // ollama pull llama3.3:latest
        // ollama pull llama3.1:8b
        var model = "llama3.1:8b";

        var innerClient = new OllamaChatClient(new Uri("http://localhost:11434/"), model);
        var chatClient = new ChatClientBuilder(innerClient)
            // .UseDistributedCache(
            //     new MemoryDistributedCache(
            //         Options.Create(new MemoryDistributedCacheOptions())
            //     ))
            .Build();

        while (true)
        {
            Console.Write("You: ");
            var userPrompt = Console.ReadLine();
            var message = new ChatMessage(ChatRole.User, userPrompt);

            // stream the AI response
            Console.Write(" AI: ");
            await foreach (ChatResponseUpdate item in chatClient.GetStreamingResponseAsync([message]))
            {
                Console.Write(item.Text);
            }

            Console.WriteLine();
        }
    }
}
