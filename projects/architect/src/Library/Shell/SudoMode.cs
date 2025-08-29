namespace Library.Shell;

// as it sits, we're only going to support none
// and explicit sudo. The goal will be to wrap
// the streams and have more control over
// detecting the sudo password prompt and 
// responding to it
public enum SudoMode
{
    None,        // no wrapping, TTY
    Explicit,     // explicit sudo command
    //Auto,          // best effort based on registered provider
}
