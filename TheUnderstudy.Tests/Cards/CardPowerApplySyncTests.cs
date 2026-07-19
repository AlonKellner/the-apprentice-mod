using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// The 3-argument CommonActions.Apply<TPower>(context, creature, card) overload reads the power's amount
// from a card DynamicVar keyed by the power type name, which only exists if the card constructor also
// declared WithPower<TPower>(...) / WithPowerNoTip<TPower>(...). Omitting it compiles fine but throws
// KeyNotFoundException at play time (the given key '<TPower>' was not present in the dictionary) — Reverb
// and Swing both shipped that way. This scans card source to keep the two in sync — no ModelDb/combat
// needed. Only the amount-less 3-arg form is matched; the explicit-amount overload
// (Apply<T>(ctx, creature, card, N)) doesn't read the var, so it needs no WithPower (e.g. Balanced).
public class CardPowerApplySyncTests
{
    private static string RepoRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    // Matches only the 3-argument (no explicit amount) call: Apply<T>(a, b, c) with no nested commas.
    private static readonly Regex ApplyPattern = new(@"CommonActions\.Apply<(\w+)>\(\s*[^(),]+,\s*[^(),]+,\s*[^(),]+\)");
    private static readonly Regex WithPowerPattern = new(@"WithPower(?:NoTip)?<(\w+)[,>]");

    public static IEnumerable<object[]> CardFilesUsingApply()
    {
        var cardsDir = Path.Combine(RepoRoot, "TheUnderstudyCode", "Cards");
        foreach (var file in Directory.GetFiles(cardsDir, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            if (ApplyPattern.IsMatch(text))
                yield return new object[] { file };
        }
    }

    [Theory]
    [MemberData(nameof(CardFilesUsingApply))]
    public void EveryCommonActionsApply_HasMatchingWithPower(string filePath)
    {
        var text = File.ReadAllText(filePath);
        var declared = WithPowerPattern.Matches(text).Select(m => m.Groups[1].Value).ToHashSet();
        foreach (Match m in ApplyPattern.Matches(text))
        {
            var power = m.Groups[1].Value;
            Assert.True(
                declared.Contains(power),
                $"{Path.GetFileName(filePath)} calls CommonActions.Apply<{power}> but never declares " +
                $"WithPower<{power}> / WithPowerNoTip<{power}> — this throws KeyNotFoundException at play time.");
        }
    }
}
