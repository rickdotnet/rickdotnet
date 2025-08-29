using Microsoft.Extensions.AI;

namespace Dusty.Cli.Demos;

public class ChatDemo
{
    public static async Task Run()
    {
        // ollama pull llama3.3:latest
        //var model = "llama3.3:latest";
        
        // ollama pull llama3.1:8b
        //var model = "llama3.1:8b";

        // ollama pull phi3:mini
        // ollama pull qwen:0.5b
        var model = "qwen:0.5b";
        
        var chatClient = new OllamaChatClient(new Uri("http://localhost:11434/"), model);
        
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
