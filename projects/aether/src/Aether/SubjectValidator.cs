namespace Aether;

public class SubjectValidator
{
    public static bool IsValid(string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
            return false;
        
        // Check length (NATS has practical limits)
        if (subject.Length > 255)
            return false;

        // Check for invalid characters
        // NATS subjects can contain: a-z, A-Z, 0-9, '.', '*', '>', '-', '_'
        foreach (char c in subject)
        {
            if (!IsValidSubjectCharacter(c))
                return false;
        }

        // Check for invalid patterns
        if (subject.StartsWith('.') || subject.EndsWith('.'))
            return false;

        if (subject.Contains(".."))
            return false;

        // Check wildcard rules
        if (subject.Contains('*') && subject.Contains('>'))
            return false; // Can't mix wildcards in same segment

        // '>' can only be at the end and must be preceded by '.'
        var gtIndex = subject.IndexOf('>');
        if (gtIndex != -1)
        {
            if (gtIndex != subject.Length - 1) // Must be at end
                return false;
            
            if (gtIndex > 0 && subject[gtIndex - 1] != '.') // Must be preceded by '.'
                return false;
        }

        return true;
    }

    private static bool IsValidSubjectCharacter(char c)
    {
        return char.IsLetterOrDigit(c) || c == '.' || c == '*' || c == '>' || c == '-' || c == '_';
    }
}