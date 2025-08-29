using System.ClientModel;
using Dusty.Shared;
using Microsoft.Extensions.AI;
using OpenAI;

namespace Dusty.Cli.Demos;

public class Search
{
    public static async Task Run()
    {
        var config = new AppConfig();
        var innerClient = new OpenAIClient(
                new ApiKeyCredential(config.GroqApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://api.groq.com/openai/v1")
                })
            .GetChatClient("compound-beta")
            .AsIChatClient();

        var chatClient = new ChatClientBuilder(innerClient)
            .Build();
        
        List<ChatMessage> chatHistory = new();
        while (true)
        {
            Console.Write("You: ");
            var userPrompt = Console.ReadLine();
            chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));
            
            Console.Write(" AI: ");
            var response = "";
            await foreach (ChatResponseUpdate item in
                           chatClient.GetStreamingResponseAsync(chatHistory))
            {
                Console.Write(item.Text);
                response += item.Text;
            }
            chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
            Console.WriteLine();
        }
    }
}