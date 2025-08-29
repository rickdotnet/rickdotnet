using Dusty.Cli.Chat;
using Dusty.Shared.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Extensions;

namespace Dusty.Cli.Demos;

public class Dusty
{
    public static async Task Run(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        var config = builder.AddConfig();
        
        if(!Directory.Exists(config.DocumentsPath))
            Directory.CreateDirectory(config.DocumentsPath);

        builder.AddLogging();
        builder.Services.AddAI(config);
        builder.Services.AddMessaging(config);

        var host = builder.Build();
        using var scope = host.Services.CreateScope();

        // requires NATS
        // var aether = scope.ServiceProvider.GetRequiredService<AetherClient>();
        // Foreman.Aether = aether;

        var chatClient = scope.ServiceProvider.GetRequiredService<IChatClient>();
        ChatDisplay chatDisplay = new();
        ChatSession chatSession = new(chatClient);

        chatSession.ModeChanged += chatDisplay.PrintChatMode;
        chatSession.ToolExecuting += chatDisplay.PrintToolCall;
        chatSession.ResponseCompleted += chatDisplay.PrintFinalResponse;

        chatDisplay.PrintChatMode(chatSession.State.Mode);

        while (true)
        {
            chatDisplay.PrintUserPrompt();
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            var chatCommand = ChatSession.ParseCommand(input);

            if (!ChatSession.IsValidCommand(chatCommand))
            {
                chatDisplay.PrintError("Invalid command.");
                continue;
            }

            try
            {
                var isChat = chatCommand.CommandType is CommandType.ChatQuery or CommandType.ToolQuery;
                if (isChat && !string.IsNullOrEmpty(chatCommand.Text))
                {
                    chatDisplay.PrintDustyHeader();
                }

                var processed = await chatSession.ProcessCommandAsync(chatCommand).Spinner();
                if (processed && isChat)
                {
                    chatDisplay.PrintSeparator();
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                chatDisplay.PrintError(ex.Message);
            }
        }
    }
}
