using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Character;

// General guardrail: every core mechanic must remain represented at the rarities where it belongs.
// This is the executable statement of the deck's rarity intent:
//   * Common must give an on-ramp to all three core builds (Invert/Planned/Tuned), preview all
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
            typeof(Choreography), typeof(EnjoyTheRide), typeof(RollWithIt), typeof(Joke),
            typeof(BrightSide), typeof(Apathy), typeof(StrikeAPose), typeof(DoubleTime),
            typeof(HeldNote), typeof(MyOwnLesson), typeof(OwnIt), typeof(LivingTheDream),
        },
        ["Planned"] = new[]
        {
            typeof(Orchestration), typeof(Foreshadow), typeof(Choreography), typeof(WriteItDown),
            typeof(Muse), typeof(Showtime), typeof(Signature), typeof(Remix),
            typeof(Melody), typeof(SellOut), typeof(Venue),
            typeof(CleanSlate), typeof(DaCapo), typeof(MagnumOpus), typeof(Motif), typeof(Playlist),
        },
        ["Tuned"] = new[]
        {
            typeof(WriteItDown), typeof(TuningRitual), typeof(RunThrough),
            typeof(Rehearse), typeof(Signature), typeof(Memorize), typeof(Perfectionism),
            typeof(CleanSlate), typeof(Experience), typeof(OneUp), typeof(StartOver),
            typeof(MuscleMemory), typeof(BackOfMyHand), typeof(Showstopper), typeof(AutoTune),
        },
        ["Weak"] = new[]
        {
            typeof(FreezeUp), typeof(DesperateStrike), typeof(WritersBlock),
            typeof(Pathos), typeof(CenterStage), typeof(FolkSong), typeof(TheFirstLesson),
        },
        ["Vulnerable"] = new[]
        {
            typeof(BreakALeg), typeof(Joke), typeof(HeartAche), typeof(TheWall),
            typeof(Pathos), typeof(CenterStage), typeof(LoveSong), typeof(TheFirstLesson),
        },
        ["Shaken"] = new[]
        {
            typeof(TheShakes), typeof(MissedCue), typeof(StageFright),
            typeof(CenterStage), typeof(StartOver), typeof(SadSong),
        },
        ["Jaded"] = new[]
        {
            typeof(RunningOnFumes), typeof(AllNighter), typeof(MustGoOn), typeof(Procrastinate),
            typeof(CenterStage), typeof(PopSong),
        },
        ["Limited"] = new[]
        {
            typeof(BuyTime), typeof(CribNotes), typeof(DrawingBlanks), typeof(Blackout),
            typeof(CenterStage), typeof(OldSong), typeof(Playlist),
        },
        ["remove-Unplayable"] = new[]
        {
            typeof(Breather), typeof(BuyTime), typeof(Unwind), typeof(Confidence),
            typeof(Improvise), typeof(LoosenUp), typeof(Balanced),
            typeof(CleanSlate), typeof(StartOver), typeof(SecondNature),
        },
        ["Vigor"] = new[]
        {
            typeof(Crash), typeof(BreakingVoice), typeof(SonicBoom), typeof(Forte),
            typeof(Crescendo), typeof(CryingOutLoud), typeof(DeceptiveCadence), typeof(Encore),
        },
        ["damage"] = new[]
        {
            typeof(Crash), typeof(DesperateStrike), typeof(AllNighter), typeof(RunThrough),
            typeof(BreakALeg), typeof(BuyTime),
        },
        ["block"] = new[]
        {
            typeof(WritersBlock), typeof(FreezeUp), typeof(Foreshadow), typeof(EnjoyTheRide),
            typeof(RunningOnFumes), typeof(TheShakes),
        },
        ["draw"] = new[]
        {
            typeof(Orchestration), typeof(Breather), typeof(RollWithIt), typeof(DrawingBlanks),
            typeof(Rehearse), typeof(AllNighter),
        },
        ["energy"] = new[]
        {
            typeof(MissedCue), typeof(MustGoOn), typeof(Forte),
            typeof(Showstopper),
        },
    };

    private static CardRarity RarityOf(Type t) =>
        ((UnderstudyCard)Activator.CreateInstance(t)!).Rarity;

    [Theory]
    // Common must cover the full preview set: 3 builds + 5 debuffs + remove-Unplayable + Vigor + 4 game mechanics.
    [InlineData("Common", "Invert")]
    [InlineData("Common", "Planned")]
    [InlineData("Common", "Tuned")]
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
    // Uncommon must represent EVERY mechanic too (same full set as Common) — no build, debuff, or
    // core game mechanic may be locked to a single tier.
    [InlineData("Uncommon", "Invert")]
    [InlineData("Uncommon", "Planned")]
    [InlineData("Uncommon", "Tuned")]
    [InlineData("Uncommon", "Weak")]
    [InlineData("Uncommon", "Vulnerable")]
    [InlineData("Uncommon", "Shaken")]
    [InlineData("Uncommon", "Jaded")]
    [InlineData("Uncommon", "Limited")]
    [InlineData("Uncommon", "remove-Unplayable")]
    [InlineData("Uncommon", "Vigor")]
    [InlineData("Uncommon", "damage")]
    [InlineData("Uncommon", "block")]
    [InlineData("Uncommon", "draw")]
    [InlineData("Uncommon", "energy")]
    // Rare must still represent all three builds + all five debuffs.
    [InlineData("Rare", "Invert")]
    [InlineData("Rare", "Planned")]
    [InlineData("Rare", "Tuned")]
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
