namespace Dusty.Cli.Chat;

public enum ChatMode
{
    Undefined = 0,
    Chat,          // 1 - chat mode, no tools
    Tools,         // 2 - tool mode, use created tools
    CompoundBeta   // 3 - compound-beta mode, use groq compound model
}
