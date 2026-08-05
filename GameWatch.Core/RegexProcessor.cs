using System.Text.RegularExpressions;

namespace GameWatch.Core;

public static class RegexProcessor
{
    public static bool IsValidPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        try
        {
            _ = new Regex(pattern);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsMatch(string pattern, string valueToCheck)
    {
        try
        {
            return Regex.IsMatch(valueToCheck, pattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}