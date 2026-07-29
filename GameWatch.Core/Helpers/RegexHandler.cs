using System.Text.RegularExpressions;

namespace GameWatch.Core.Helpers;

public static class RegexHandler
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

    public static bool IsMatch(string valueToCheck, string pattern)
    {
        try
        {
            return Regex.IsMatch(valueToCheck, pattern);
        }
        catch
        {
            return false;
        }
    }
}