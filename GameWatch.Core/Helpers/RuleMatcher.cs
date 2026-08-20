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

        // Validation rule check
        if ((hasTitleExact && hasTitleRule) || (hasPathExact && hasPathRule))
            ThrowInvalidRuleCombination(gameRecord);

        if (!hasTitleExact && !hasTitleRule && !hasPathExact && !hasPathRule)
            ThrowNoRulesConfigured(gameRecord);

        // Evaluate Title Match (Window titles are always case-insensitive across OSs)
        bool titleMatches;
        if (hasTitleExact)
        {
            titleMatches = string.Equals(gameRecord.WindowTitle, procDto.WindowTitle, StringComparison.OrdinalIgnoreCase);
        }
        else if (hasTitleRule)
        {
            titleMatches = RegexProcessor.IsMatch(gameRecord.WindowRule!, procDto.WindowTitle);
        }
        else
        {
            titleMatches = true;
        }

        // Short-circuit: if title doesn't match, skip path rule overhead entirely
        if (!titleMatches)
            return false;

        // Evaluate Path Match using OS-specific path comparison semantics
        if (hasPathExact)
        {
            return string.Equals(gameRecord.FilePath, procDto.FilePath, ProcGatherer.PathComparison);
        }

        if (hasPathRule)
        {
            return RegexProcessor.IsMatch(gameRecord.PathRule!, procDto.FilePath);
        }

        return true;
    }

    private static void ThrowInvalidRuleCombination(AutoGameRecord gameRecord)
    {
        throw new ArgumentException(
            $"[FATAL ERROR] Game with TableId='{gameRecord.TableId}' Name='{gameRecord.Name}' " +
            $"cannot have both a rule version and exact match version for the same field.\n" +
            $"Matching rules:\n" +
            $"* Window Title: {gameRecord.WindowTitle}\n" +
            $"* Window Rule: {gameRecord.WindowRule}\n" +
            $"* File Path: {gameRecord.FilePath}\n" +
            $"* Path Rule: {gameRecord.PathRule}");
    }

    private static void ThrowNoRulesConfigured(AutoGameRecord gameRecord)
    {
        throw new ArgumentException(
            $"[FATAL ERROR] Game with TableId='{gameRecord.TableId}' Name='{gameRecord.Name}' " +
            $"must have at least one matching rule configured.\n" +
            $"Matching rules:\n" +
            $"* Window Title: {gameRecord.WindowTitle}\n" +
            $"* Window Rule: {gameRecord.WindowRule}\n" +
            $"* File Path: {gameRecord.FilePath}\n" +
            $"* Path Rule: {gameRecord.PathRule}");
    }
}