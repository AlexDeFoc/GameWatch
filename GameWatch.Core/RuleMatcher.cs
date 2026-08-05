using System;
using GameWatch.Core.Dto;

namespace GameWatch.Core;

public static class RuleMatcher
{
    public static bool IsMatch(OurProc proc, GameRecords.AutoGame game)
    {
        var hasTitleExact = game.WindowTitle != null;
        var hasTitleRule = game.WindowRule != null;
        var hasPathExact = game.FilePath != null;
        var hasPathRule = game.PathRule != null;

        // Rule version cannot coexist with exact match version for the same field
        if ((hasTitleExact && hasTitleRule) || (hasPathExact && hasPathRule))
        {
            throw new NotImplementedException();
        }

        // Must have at least 1 rule configured
        if (!hasTitleExact && !hasTitleRule && !hasPathExact && !hasPathRule)
        {
            throw new NotImplementedException();
        }

        var titleMatches = (!hasTitleExact && !hasTitleRule) ||
                            (hasTitleExact
                                ? game.WindowTitle == proc.WindowTitle
                                : RegexProcessor.IsMatch(game.WindowRule!, proc.WindowTitle));

        var pathMatches = (!hasPathExact && !hasPathRule) ||
                           (hasPathExact
                               ? game.FilePath == proc.FilePath
                               : RegexProcessor.IsMatch(game.PathRule!, proc.FilePath));

        return titleMatches && pathMatches;
    }
}