using Dusty.Shared.Prompts;
using Dusty.Shared.Tools;
using Microsoft.Extensions.AI;
using Spectre.Console;

namespace Dusty.Cli.Chat;

public class ChatSession
{
    private readonly IChatClient chatClient;
    private readonly List<AITool> tools;

    public ChatSessionState State { get; private set; }

    public event Action<ChatMode>? ModeChanged;
    public event Action<FunctionCallContent>? ToolExecuting;
    public event Action<string>? ResponseCompleted;

    public ChatSession(IChatClient chatClient)
    {
        this.chatClient = chatClient;

        tools = [];
        tools.AddRange(Gist.Tools);
        tools.AddRange(Documents.Tools);
        //tools.AddRange(Foreman.Tools); // requires NATS

        State = new ChatSessionState(DustyPrompts.Chat);
    }

    private void ChangeMode(ChatMode newMode)
    {
        if (State.Mode == newMode)
            return;

        State.SetMode(newMode);
        ModeChanged?.Invoke(newMode);
    }

    private void SelectTools()
    {
        var selectedTools = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select [green]tools[/]?")
                .NotRequired()
                .PageSize(10)
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a tool, " + 
                    "[green]<enter>[/] to accept)[/]")
                .AddChoiceGroup("Documents", "List Files", "Get Content", "Save Content")
                .AddChoiceGroup("Other Tools", "Search", "Cheese")
                .Select("List Files")
                .Select("Get Content")
                .Select("Save Content")
                .Select("Search")
                .Select("Cheese")
                .Select("Documents")
                .Select("Other Tools")
            );
        
        AnsiConsole.MarkupLine($"[blue]You selected: {string.Join(", ", selectedTools)}[/]");
    }

    public Task<bool> ProcessCommandAsync(ChatCommand command)
    {
        switch (command.CommandType)
        {
            case CommandType.Shell:
                return Task.FromResult(ProcessShellCommand(command));
            case CommandType.Session:
                return Task.FromResult(ProcessSessionCommand(command));
            case CommandType.ToolQuery:
            case CommandType.ChatQuery:
                return ProcessChatQueryAsync(command);
            case CommandType.Undefined:
            default:
                AnsiConsole.MarkupLine("[red]Invalid command type.[/]");
                return Task.FromResult(false);
        }
    }

    private bool ProcessSessionCommand(ChatCommand command)
    {
        var sessionCommand = command.Text?.TrimStart(':') ?? string.Empty;
        switch (sessionCommand)
        {
            case "q" or "quit" or "exit":
                AnsiConsole.MarkupLine("[red]Exiting session...[/]");
                Environment.Exit(0);
                return false;
            case "t":
                ChangeMode(ChatMode.Tools);
                return true;
            case "tools":
                SelectTools();
                return true;
            case "c" or "chat":
                ChangeMode(ChatMode.Chat);
                return true;
            case "comp" or "compound":
                ChangeMode(ChatMode.CompoundBeta);
                return true;
            default:
                return false;
        }
    }

    private async Task<bool> ProcessChatQueryAsync(ChatCommand command)
    {
        // Add user message
        var userMessage = new ChatMessage(ChatRole.User, command.Text!);
        State.AddMessage(userMessage);

        // Determine if tools should be used
        var useTools = State.Mode == ChatMode.Tools || command.CommandType == CommandType.ToolQuery;
        var options = useTools
            ? new ChatOptions
            {
                Tools = tools,
                ToolMode = ChatToolMode.RequireAny,
            }
            : null;


        var responseText = new TextContent("");
        var currentResponseMessage = new ChatMessage(ChatRole.Assistant, [responseText]);

        try
        {
            await foreach (var update in chatClient.GetStreamingResponseAsync(State.Messages, options))
            {
                foreach (var content in update.Contents)
                {
                    switch (content)
                    {
                        case FunctionCallContent functionCall:
                        {
                            var contentFilter = update.Contents.Where(c => c is not TextContent).ToList();
                            var updateMessage = new ChatMessage(update.Role ?? ChatRole.Assistant, contentFilter)
                            {
                                AuthorName = update.AuthorName,
                                RawRepresentation = update.RawRepresentation,
                                AdditionalProperties = update.AdditionalProperties
                            };

                            State.AddMessage(updateMessage);
                            ToolExecuting?.Invoke(functionCall);
                            break;
                        }
                        case TextContent textContent when !string.IsNullOrEmpty(textContent.Text):
                            responseText.Text += update.Text;
                            break;
                    }
                }
            }

            State.AddMessage(currentResponseMessage);
            ResponseCompleted?.Invoke(responseText.Text);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            var errorMessage = new ChatMessage(ChatRole.Assistant, $"Error processing request: {ex.Message}");
            State.AddMessage(errorMessage);

            ResponseCompleted?.Invoke(responseText.Text);
            return false;
        }

        return true;
    }


    private bool ProcessShellCommand(ChatCommand command)
    {
        AnsiConsole.MarkupLine($"[green]Running shell command: {command}[/]");
        AnsiConsole.Write(new FigletText("Dusty"));
        return true;
    }
    
    public static ChatCommand ParseCommand(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new ChatCommand();

        input = input.Trim();

        // Shell command
        if (input.StartsWith("::", StringComparison.OrdinalIgnoreCase))
        {
            var command = input[2..].Trim();
            return new ChatCommand { CommandType = CommandType.Shell, Text = command };
        }

        // Tool query
        if (input.StartsWith(":>"))
        {
            var query = input[2..].Trim();
            return new ChatCommand { CommandType = CommandType.ToolQuery, Text = query };
        }

        // session
        if (input.StartsWith(':'))
        {
            return new ChatCommand{ CommandType = CommandType.Session, Text = input };
        }

        // chat
        return new ChatCommand { CommandType = CommandType.ChatQuery, Text = input };
    }

    public static bool IsValidCommand(ChatCommand command)
    {
        switch (command.CommandType)
        {
            case CommandType.Shell:
            case CommandType.ChatQuery:
            case CommandType.ToolQuery:
            case CommandType.Session:
                return !string.IsNullOrEmpty(command.Text);
            case CommandType.Undefined:
            default:
                return false;
        }
    }
}