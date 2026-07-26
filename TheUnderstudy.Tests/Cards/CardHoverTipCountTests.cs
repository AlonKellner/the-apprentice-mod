using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BaseLib.Abstracts;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Guards the WithPowerNoTip helper, which is easy to break silently.
//
// BaseLib's ConstructedCardModel.WithVars scans every dynamic var it is given and auto-registers a
// power tooltip for each generic type argument assignable to PowerModel. PowerVar<T> therefore tips
// itself, and the original "just don't call WithTip" implementation of WithPowerNoTip did nothing at
// all — ~19 Power cards shipped showing a tooltip that merely restated their own card text. The fix
// removes the auto-added tip after the fact, which is exactly the kind of thing that can regress
// unnoticed on a BaseLib upgrade.
//
// So: assert the number of static hover tips a card actually ends up with equals the number it asks
// for in its own source. Bare instantiation only (no ModelDb, same constraint as NewDeckCardsTests),
// and the tip COUNT is read without ever evaluating a tip — HoverTipFactory needs ModelDb and would
// crash the bare test host.
public class CardHoverTipCountTests
{
    // ConstructedCardModel keeps two separate lists. _hoverTips holds the static WithTip(...) sources
    // (and the auto-added power tips this test is about); dynamic WithTips(lambda) / WithMarkedTip
    // sources live in _multiHoverTips and are deliberately not counted here.
    private static readonly FieldInfo HoverTipsField =
        typeof(ConstructedCardModel).GetField("_hoverTips", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static string RepoRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    private static string CardsDir => Path.Combine(RepoRoot, "TheUnderstudyCode", "Cards");

    // Both calls append exactly one entry to _hoverTips. "WithTips(" and "WithMarkedTip(" do not
    // contain "WithTip(" as a substring, so neither is miscounted here.
    private static int DeclaredTipCount(string source) =>
        Occurrences(source, "WithTip(") + Occurrences(source, "WithPower<");

    private static int Occurrences(string haystack, string needle)
    {
        int count = 0;
        for (int i = haystack.IndexOf(needle, StringComparison.Ordinal); i >= 0;
             i = haystack.IndexOf(needle, i + needle.Length, StringComparison.Ordinal))
            count++;
        return count;
    }

    public static IEnumerable<object[]> ConcreteCards() =>
        typeof(UnderstudyCard).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(UnderstudyCard)) && !t.IsAbstract)
            .Select(t => new object[] { t })
            .OrderBy(o => ((Type)o[0]).Name);

    [Theory]
    [MemberData(nameof(ConcreteCards))]
    public void Card_StaticHoverTipCount_MatchesItsSource(Type cardType)
    {
        var path = Path.Combine(CardsDir, cardType.Name + ".cs");
        Assert.True(File.Exists(path), $"No source file found for card {cardType.Name} at {path}.");

        var card = (UnderstudyCard)Activator.CreateInstance(cardType)!;
        int actual = ((IList)HoverTipsField.GetValue(card)!).Count;
        int expected = DeclaredTipCount(File.ReadAllText(path));

        Assert.True(actual == expected,
            $"{cardType.Name} declares {expected} static hover tip(s) in its source but ends up with "
            + $"{actual}. A surplus usually means BaseLib auto-added a power tooltip that "
            + "WithPowerNoTip failed to remove (see UnderstudyCard.WithPowerNoTip).");
    }

    // The specific regression that motivated the helper: a Power card whose text already states its
    // effect in plain mechanical language must carry no power tooltip at all.
    [Theory]
    [InlineData(typeof(AnotherBrick))]
    [InlineData(typeof(MasterForm))]
    [InlineData(typeof(HeldNote))]
    public void PowerCard_WithNoDeclaredTips_HasNoHoverTip(Type cardType)
    {
        var card = (UnderstudyCard)Activator.CreateInstance(cardType)!;
        Assert.Empty((IList)HoverTipsField.GetValue(card)!);
    }

    // ...while the two Lesson cards whose text is flavour rather than mechanics keep theirs.
    [Theory]
    [InlineData(typeof(TheSecondLesson))]
    [InlineData(typeof(TheFinalLesson))]
    public void LessonCard_KeepsItsPowerTooltip(Type cardType)
    {
        var card = (UnderstudyCard)Activator.CreateInstance(cardType)!;
        Assert.NotEmpty((IList)HoverTipsField.GetValue(card)!);
    }
}
