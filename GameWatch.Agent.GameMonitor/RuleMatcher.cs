using System;
using GameWatch.Core.Dto;
using GameWatch.Core.Dto.GameRecords;
using GameWatch.Core.Helpers;

namespace GameWatch.Agent.GameMonitor;

public static class RuleMatcher
{
    public static bool IsMatch(OurProc candidate, AutoGame rule)
    {
        if (rule is { ProcessWindowTitle: not null, ProcessWindowTitlePattern: null })
        {
            if (!string.Equals(candidate.WindowTitle, rule.ProcessWindowTitle, StringComparison.Ordinal))
                return false;
        }

        if (rule is { ProcessFilePath: not null, ProcessFilePathPattern: null })
        {
            if (!string.Equals(candidate.FilePath, rule.ProcessFilePath, ProcessFinder.PathComparison))
                return false;
        }

        if (rule.ProcessWindowTitlePattern != null)
        {
            if (!RegexHandler.IsMatch(candidate.WindowTitle, rule.ProcessWindowTitlePattern))
                return false;
        }

        // ReSharper disable once InvertIf
        if (rule.ProcessFilePathPattern != null)
        {
            if (!RegexHandler.IsMatch(candidate.FilePath, rule.ProcessFilePathPattern))
                return false;
        }

        // Will always have a rule to match against
        return true;
    }
}