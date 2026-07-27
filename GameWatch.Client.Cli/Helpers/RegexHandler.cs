using System.Text.RegularExpressions;

namespace GameWatch.Client.Cli.Helpers;

public static class RegexHandler
{
    public static bool ValidatePattern(string pattern)
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
}