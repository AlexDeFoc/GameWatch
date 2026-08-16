using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace GameWatch.Core.Helpers;

public static class RegexProcessor
{
    private static readonly ConcurrentDictionary<string, Regex?> RegexCache = new(StringComparer.Ordinal);
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(100);

    public static bool IsValidPattern(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        return GetOrAddRegex(pattern) is not null;
    }

    public static bool IsMatch(string pattern, string valueToCheck)
    {
        if (string.IsNullOrEmpty(valueToCheck))
            return false;

        var regex = GetOrAddRegex(pattern);
        if (regex is null)
            return false;

        try
        {
            return regex.IsMatch(valueToCheck);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private static Regex? GetOrAddRegex(string pattern)
    {
        return RegexCache.GetOrAdd(pattern, static p =>
        {
            try
            {
                // RegexOptions.CultureInvariant avoids culture-sensitive comparison overhead
                return new Regex(p, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, DefaultTimeout);
            }
            catch
            {
                return null;
            }
        });
    }
}