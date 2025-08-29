using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Dusty.Cli;

public class MarkdownToSpectreMapper
{
    private readonly MarkdownPipeline pipeline;

    public MarkdownToSpectreMapper()
    {
        pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public string ConvertToSpectreMarkup(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return string.Empty;

        var document = Markdown.Parse(markdown, pipeline);
        var spectreMarkup = new StringBuilder();
        ProcessBlock(document, spectreMarkup);
        return spectreMarkup.ToString();
    }

    private void ProcessBlock(MarkdownObject node, StringBuilder spectreMarkup)
    {
        switch (node)
        {
            case HeadingBlock heading:
                ProcessHeading(heading, spectreMarkup);
                break;
            case ParagraphBlock paragraph:
                ProcessInlines(paragraph.Inline, spectreMarkup);
                spectreMarkup.AppendLine();
                break;
            case FencedCodeBlock fencedCode:
                ProcessFencedCode(fencedCode, spectreMarkup);
                spectreMarkup.AppendLine();
                break;
            default:
                if (node is not ContainerBlock containerBlock)
                    return;
                
                foreach (var child in containerBlock)
                    ProcessBlock(child, spectreMarkup);

                break;
        }
    }

    private void ProcessHeading(HeadingBlock heading, StringBuilder spectreMarkup)
    {
        // Map heading levels to bold and color styles
        string style = heading.Level switch
        {
            1 => "bold yellow",
            2 => "bold green",
            3 => "bold cyan",
            _ => "bold"
        };
        spectreMarkup.Append($"[{style}]");
        ProcessInlines(heading.Inline, spectreMarkup);
        spectreMarkup.Append("[/]");
        spectreMarkup.AppendLine();
    }

    private void ProcessInlines(ContainerInline? container, StringBuilder spectreMarkup)
    {
        if (container == null)
            return;

        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    spectreMarkup.Append(EscapeMarkup(literal.Content.ToString()));
                    break;
                case EmphasisInline emphasis:
                    ProcessEmphasis(emphasis, spectreMarkup);
                    break;
                case LinkInline link:
                    ProcessLink(link, spectreMarkup);
                    break;
                case CodeInline codeInline:
                    // Spectre.Console uses [dim] for inline code
                    spectreMarkup.Append("[blue]");
                    spectreMarkup.Append(EscapeMarkup(codeInline.Content));
                    spectreMarkup.Append("[/]");
                    break;
                default:
                    // Process nested inlines recursively
                    if (inline is ContainerInline containerInline)
                    {
                        ProcessInlines(containerInline, spectreMarkup);
                    }

                    break;
            }
        }
    }

    private void ProcessFencedCode(FencedCodeBlock codeBlock, StringBuilder spectreMarkup)
    {
        // Spectre.Console uses [code] tags for code blocks
        spectreMarkup.AppendLine($"```{codeBlock.Info}");
        spectreMarkup.AppendLine(EscapeMarkup(codeBlock.Lines.ToString()));
        spectreMarkup.AppendLine("```");
    }

    private void ProcessEmphasis(EmphasisInline emphasis, StringBuilder spectreMarkup)
    {
        string style = emphasis.DelimiterChar switch
        {
            '*' when emphasis.DelimiterCount == 2 => "bold yellow",
            '*' when emphasis.DelimiterCount == 1 => "italic blue",
            '_' when emphasis.DelimiterCount == 1 => "italic blue",
            _ => ""
        };

        if (!string.IsNullOrEmpty(style))
        {
            spectreMarkup.Append($"[{style}]");
            ProcessInlines(emphasis, spectreMarkup);
            spectreMarkup.Append("[/]");
        }
        else
        {
            ProcessInlines(emphasis, spectreMarkup);
        }
    }

    private void ProcessLink(LinkInline link, StringBuilder spectreMarkup)
    {
        // Links in Spectre.Console are styled with underline and blue
        spectreMarkup.Append("[underline blue]");
        ProcessInlines(link, spectreMarkup);
        spectreMarkup.Append("[/]");
        // Optionally append the URL in parentheses
        spectreMarkup.Append($" ([link]{EscapeMarkup(link.Url)}[/])");
    }

    private string EscapeMarkup(string? text)
    {
        // Escape Spectre.Console markup characters [ and ]
        return text?.Replace("[", "[[").Replace("]", "]]") ?? string.Empty;
    }
}
