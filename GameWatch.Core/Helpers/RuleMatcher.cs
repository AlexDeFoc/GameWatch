using System;
using GameWatch.Core.Types;

namespace GameWatch.Core.Helpers;

public static class RuleMatcher
{
    public static bool IsMatch(ProcDto procDto, AutoGameRecord gameRecord)
    {
        var hasTitleExact = gameRecord.WindowTitle != null;
        var hasTitleRule = gameRecord.WindowRule != null;
        var hasPathExact = gameRecord.FilePath != null;
        var hasPathRule = gameRecord.PathRule != null;

        // Rule version cannot coexist with exact match version for the same field
        if ((hasTitleExact && hasTitleRule) || (hasPathExact && hasPathRule))
        {
            throw new ArgumentException($"[FATAL ERROR] GameRecord with TableId='{gameRecord.TableId}' Name='{gameRecord.Name}' " +
                                        $"cannot have both a rule version and exact match version for the same field. " +
                                        $"Matching rules:\n" +
                                        $"Window Title: {gameRecord.WindowTitle}\n" +
                                        $"Window Rule: {gameRecord.WindowRule}\n" +
                                        $"File Path: {gameRecord.FilePath}\n" +
                                        $"Path Rule: {gameRecord.PathRule}");
        }

        // Must have at least 1 rule configured
        if (!hasTitleExact && !hasTitleRule && !hasPathExact && !hasPathRule)
        {
            throw new ArgumentException($"[FATAL ERROR] GameRecord with TableId='{gameRecord.TableId}' Name='{gameRecord.Name}' " +
                                        $"must have at least one matching rule configured. " +
                                        $"Matching rules:\n" +
                                        $"Window Title: {gameRecord.WindowTitle}\n" +
                                        $"Window Rule: {gameRecord.WindowRule}\n" +
                                        $"File Path: {gameRecord.FilePath}\n" +
                                        $"Path Rule: {gameRecord.PathRule}");
        }

        var titleMatches = (!hasTitleExact && !hasTitleRule) ||
                           (hasTitleExact
                               ? gameRecord.WindowTitle == procDto.WindowTitle
                               : RegexProcessor.IsMatch(gameRecord.WindowRule!, procDto.WindowTitle));

        var pathMatches = (!hasPathExact && !hasPathRule) ||
                          (hasPathExact
                              ? gameRecord.FilePath == procDto.FilePath
                              : RegexProcessor.IsMatch(gameRecord.PathRule!, procDto.FilePath));

        return titleMatches && pathMatches;
    }
}