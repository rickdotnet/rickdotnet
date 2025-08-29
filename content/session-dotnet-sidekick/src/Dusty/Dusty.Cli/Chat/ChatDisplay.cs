using Microsoft.Extensions.AI;
using Spectre.Console;

namespace Dusty.Cli.Chat;

public class ChatDisplay
{
    private readonly MarkdownToSpectreMapper markdownMapper = new();
    private readonly Rule dustyRule = new Rule("Dusty").LeftJustified().RuleStyle("red");
    private readonly Rule greenRule = new Rule().LeftJustified().RuleStyle("green");

    public void PrintChatMode(ChatMode mode)
    {
        Rule rule = mode switch
        {
            ChatMode.Chat => new("[green]Chat Mode[/]"),
            ChatMode.Tools => new("[blue]Tools Mode[/]"),
            ChatMode.CompoundBeta => new("[yellow]Compound Mode[/]"),
            _ => new("[red]Unknown Mode[/]")
        };

        AnsiConsole.Write(rule);
    }

    public void PrintUserPrompt()
    {
        AnsiConsole.Markup("[green]You:[/] ");
    }

    public void PrintDustyHeader()
    {
        AnsiConsole.Write(dustyRule);
    }

    public void PrintSeparator()
    {
        AnsiConsole.Write(greenRule);
    }

    public void PrintToolCall(FunctionCallContent functionCall)
    {
        AnsiConsole.MarkupLine("Calling tool: [blue]{0}[/]", functionCall.Name);
    }

    public void PrintFinalResponse(string responseText)
    {
        if (string.IsNullOrEmpty(responseText))
            return;

        var spectreMarkup = markdownMapper.ConvertToSpectreMarkup(responseText);
        AnsiConsole.Markup(spectreMarkup);
    }

    public void PrintError(string message)
    {
        AnsiConsole.MarkupLine($"[red]Error: {message}[/]");
    }
}
