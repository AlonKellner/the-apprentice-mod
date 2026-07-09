using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Character;

// General guardrail: every core mechanic must remain represented at the rarities where it belongs.
// This is the executable statement of the deck's rarity intent:
//   * Common must give an on-ramp to all three core builds (Invert/Planned/Tense), preview all
//     five debuffs + remove-Unplayable + Vigor, and cover the four core game mechanics
//     (damage/block/draw/energy).
//   * Uncommon and Rare must each still represent all three builds and all five debuffs, so no
//     build is locked out of a tier.
// Rarity is read live from a bare-instantiated card (same safe pattern as NewDeckCardsTests —
// no ModelDb/Log paths), so re-rartying a card in source automatically re-computes coverage.
public class CardRarityMechanicCoverageTests
{
    // mechanic -> the cards that embody it (curated from the card set; rarity-agnostic).
    // Lists are kept reasonably complete so the guardrail survives future single-card re-rarities
    // without false alarms. damage/block/draw only need enough representatives to cover Common.
    private static readonly Dictionary<string, Type[]> MechanicCards = new()
    {
        ["Invert"] = new[]
        {
            typeof(PlotTwist), typeof(SteadyNow), typeof(TakeABreath), typeof(TrueColors),
            typeof(AdLib), typeof(PulledPunch), typeof(Coda), typeof(DoubleTime),
            typeof(HeldNote), typeof(MyOwnLesson), typeof(EverythingIveGot),
        },
        ["Planned"] = new[]
        {
            typeof(Cue), typeof(Foreshadow), typeof(PlotTwist), typeof(CramSession),
            typeof(CallSheet), typeof(CurtainCall), typeof(Prompt), typeof(Remix),
            typeof(Arrangement), typeof(FinalBar), typeof(StageManager),
            typeof(CleanSlate), typeof(Encore), typeof(MagnumOpus), typeof(Refrain), typeof(TableRead),
        },
        ["Tense"] = new[]
        {
            typeof(CramSession), typeof(TouchUp), typeof(Reprise),
            typeof(Rehearse), typeof(StandingOvation), typeof(StageFright), typeof(WarmUp),
            typeof(CleanSlate), typeof(CutTheTension), typeof(DaCapo), typeof(MissedCue),
            typeof(MuscleMemory), typeof(NervousEnergy), typeof(Showstopper),
        },
        ["Weak"] = new[]
        {
            typeof(FreezeUp), typeof(StageWhisper), typeof(Understatement),
            typeof(Ensemble), typeof(DressRehearsal), typeof(FolkSong), typeof(TheFirstLesson),
        },
        ["Vulnerable"] = new[]
        {
            typeof(Downstage), typeof(TrueColors), typeof(Overcommit), typeof(WideOpen),
            typeof(Ensemble), typeof(DressRehearsal), typeof(LoveSong), typeof(TheFirstLesson),
        },
        ["Shaken"] = new[]
        {
            typeof(Butterflies), typeof(OpeningNumber), typeof(TakeCenterStage),
            typeof(DressRehearsal), typeof(MissedCue), typeof(SadSong),
        },
        ["Jaded"] = new[]
        {
            typeof(Matinee), typeof(QuickNap), typeof(AllNighter), typeof(Rerun),
            typeof(DressRehearsal), typeof(PopSong),
        },
        ["Limited"] = new[]
        {
            typeof(Flourish), typeof(OffScript), typeof(FastForward), typeof(Overexert),
            typeof(DressRehearsal), typeof(OldSong), typeof(TableRead),
        },
        ["remove-Unplayable"] = new[]
        {
            typeof(Rewrite), typeof(TouchUp), typeof(TakeTwo), typeof(SafetyNet),
            typeof(Improvise), typeof(TakeYourBow), typeof(StandingBy),
            typeof(CleanSlate), typeof(MissedCue), typeof(SecondNature),
        },
        ["Vigor"] = new[]
        {
            typeof(Bravado), typeof(WindUp), typeof(BigBreak), typeof(BreakALeg),
            typeof(Crescendo), typeof(StandingOvation), typeof(TakeNotes),
        },
        ["damage"] = new[]
        {
            typeof(Bravado), typeof(StageWhisper), typeof(QuickNap), typeof(Reprise),
            typeof(Downstage), typeof(Flourish),
        },
        ["block"] = new[]
        {
            typeof(Understatement), typeof(FreezeUp), typeof(Foreshadow), typeof(SteadyNow),
            typeof(Matinee), typeof(Butterflies),
        },
        ["draw"] = new[]
        {
            typeof(Cue), typeof(Rewrite), typeof(TakeABreath), typeof(FastForward),
            typeof(Rehearse), typeof(Prompt),
        },
        ["energy"] = new[]
        {
            typeof(OpeningNumber), typeof(AllNighter), typeof(BreakALeg),
            typeof(NervousEnergy), typeof(Showstopper),
        },
    };

    private static CardRarity RarityOf(Type t) =>
        ((UnderstudyCard)Activator.CreateInstance(t)!).Rarity;

    [Theory]
    // Common must cover the full preview set: 3 builds + 5 debuffs + remove-Unplayable + Vigor + 4 game mechanics.
    [InlineData("Common", "Invert")]
    [InlineData("Common", "Planned")]
    [InlineData("Common", "Tense")]
    [InlineData("Common", "Weak")]
    [InlineData("Common", "Vulnerable")]
    [InlineData("Common", "Shaken")]
    [InlineData("Common", "Jaded")]
    [InlineData("Common", "Limited")]
    [InlineData("Common", "remove-Unplayable")]
    [InlineData("Common", "Vigor")]
    [InlineData("Common", "damage")]
    [InlineData("Common", "block")]
    [InlineData("Common", "draw")]
    [InlineData("Common", "energy")]
    // Uncommon must still represent all three builds + all five debuffs.
    [InlineData("Uncommon", "Invert")]
    [InlineData("Uncommon", "Planned")]
    [InlineData("Uncommon", "Tense")]
    [InlineData("Uncommon", "Weak")]
    [InlineData("Uncommon", "Vulnerable")]
    [InlineData("Uncommon", "Shaken")]
    [InlineData("Uncommon", "Jaded")]
    [InlineData("Uncommon", "Limited")]
    // Rare must still represent all three builds + all five debuffs.
    [InlineData("Rare", "Invert")]
    [InlineData("Rare", "Planned")]
    [InlineData("Rare", "Tense")]
    [InlineData("Rare", "Weak")]
    [InlineData("Rare", "Vulnerable")]
    [InlineData("Rare", "Shaken")]
    [InlineData("Rare", "Jaded")]
    [InlineData("Rare", "Limited")]
    public void Rarity_Represents_Mechanic(string rarity, string mechanic)
    {
        var want = Enum.Parse<CardRarity>(rarity);
        Assert.True(
            MechanicCards[mechanic].Any(t => RarityOf(t) == want),
            $"No {rarity} card embodies '{mechanic}'.");
    }
}
