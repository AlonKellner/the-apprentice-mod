using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// A resolver's TargetType getter must NOT inline-scan other cards' TargetType (".Any(c => c.TargetType ==
// ...)"). A resolver can itself be Planned/Tuned, so it appears in the very set being scanned, and reading
// its own TargetType re-enters the getter forever — an unbounded, non-logging, allocating recursion that
// hard-crashes with a stack overflow. This was the Spectacle crash (a Tuned Spectacle's TargetType). The
// question "does any queued/tuned card need an enemy target?" must instead route through the re-entrancy-
// guarded helpers PlannedModifier.QueueNeedsEnemyTarget / TunedModifier.TunedQueueNeedsEnemyTarget (both
// gated by a [ThreadStatic] flag). Source scan of the top-level card files (the helpers live in the
// Modifiers/ subdirectory and are intentionally excluded).
public class ResolverTargetTypeGuardTests
{
    private static string CardsDir => Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "TheUnderstudyCode", "Cards"));

    [Fact]
    public void NoCard_InlineScansOtherCardsTargetType()
    {
        Assert.True(Directory.Exists(CardsDir), $"Cards dir not found: {CardsDir}");

        var offenders = new List<string>();
        foreach (var file in Directory.GetFiles(CardsDir, "*.cs", SearchOption.TopDirectoryOnly))
        {
            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
                if (Regex.IsMatch(lines[i], @"\.Any\(\s*c\s*=>\s*c\.TargetType"))
                    offenders.Add($"{Path.GetFileName(file)}:{i + 1}");
        }

        Assert.True(offenders.Count == 0,
            "A resolver's TargetType getter inline-scans other cards' TargetType and can infinitely recurse " +
            "when the resolver is itself Planned/Tuned — route it through the guarded QueueNeedsEnemyTarget / " +
            "TunedQueueNeedsEnemyTarget helpers instead. Offenders: " + string.Join(", ", offenders));
    }
}
