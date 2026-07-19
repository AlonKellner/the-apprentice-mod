using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Character;

// General guardrail: every core mechanic must remain represented at the rarities where it belongs.
// This is the executable statement of the deck's rarity intent:
//   * Common must give an on-ramp to all three core builds (Invert/Planned/Tuned), preview the
//     self-debuffs it applies (Weak/Vulnerable/Tension) + Swap + remove-Unplayable + Vigor, and
//     cover the four core game mechanics (damage/block/draw/energy).
//   * Uncommon must represent every mechanic too; Rare must still represent all three builds,
//     Weak/Vulnerable, and Swap. (Legacy Shaken/Limited/Jaded are latent — no card applies them.)
// Rarity is read live from a bare-instantiated card (same safe pattern as NewDeckCardsTests —
// no ModelDb/Log paths), so re-rartying a card in source automatically re-computes coverage.
public class CardRarityMechanicCoverageTests
{
    // mechanic -> the cards that embody it (curated from the card set; rarity-agnostic).
    // Lists are kept reasonably complete so the guardrail survives future single-card re-rarities
    // without false alarms. damage/block/draw only need enough representatives to cover Common.
    // NOTE (redesign, in progress): lists updated to the surviving/renamed cards; new cards (Upstage,
    // GiveAndTake, StagePresence, Composure, SilverLining, Muffle, DeadWeight, SingAlong, NervousEnergy,
    // TakeNotes, Cram, GoForBroke, BurnOut, ...) are added as they are created in the new-card phase.
    // "Tension" is dropped (no card applies it post-redesign; TensionPower stays latent like
    // Shaken/Jaded/Limited).
    private static readonly Dictionary<string, Type[]> MechanicCards = new()
    {
        ["Invert"] = new[]
        {
            typeof(Joke), typeof(BrightSide), typeof(Apathy),
            typeof(HeldNote), typeof(MyOwnLesson), typeof(OwnIt), typeof(LivingTheDream),
            typeof(StrikeAPose), typeof(SilverLining), typeof(GiveAndTake), typeof(TurnItAround),
        },
        ["Planned"] = new[]
        {
            typeof(Orchestration), typeof(Foreshadow), typeof(WriteItDown),
            typeof(Muse), typeof(Showtime), typeof(Signature), typeof(Remix),
            typeof(Melody), typeof(Intermission),
            typeof(CleanSlate), typeof(DaCapo), typeof(MagnumOpus), typeof(Motif),
        },
        ["Tuned"] = new[]
        {
            typeof(WriteItDown), typeof(TuningRitual), typeof(RunThrough),
            typeof(Signature), typeof(Memorize), typeof(Perfectionism),
            typeof(CleanSlate), typeof(Experience), typeof(OneUp),
            typeof(MuscleMemory), typeof(BackOfMyHand), typeof(Showstopper), typeof(AutoTune),
            typeof(ShowerThought), typeof(TakeNotes),
        },
        ["Weak"] = new[]
        {
            typeof(FreezeUp), typeof(DesperateStrike),
            typeof(Pathos), typeof(FolkSong), typeof(TheFirstLesson), typeof(DeadWeight),
        },
        ["Vulnerable"] = new[]
        {
            typeof(Joke), typeof(HeartAche), typeof(TheWall),
            typeof(Pathos), typeof(LoveSong), typeof(TheFirstLesson),
            typeof(Meltdown), typeof(NervousEnergy),
        },
        ["Swap"] = new[]
        {
            typeof(RoleReversal), typeof(BodyDouble),
            typeof(Upstage), typeof(GiveAndTake), typeof(StagePresence),
        },
        ["remove-Unplayable"] = new[]
        {
            typeof(Improvise), typeof(LoosenUp), typeof(Balanced),
            typeof(CleanSlate), typeof(SecondNature),
            typeof(Comeback), typeof(Composure), typeof(TurnItAround), typeof(GoForBroke),
        },
        ["Vigor"] = new[]
        {
            typeof(Crash), typeof(BreakingVoice), typeof(SonicBoom), typeof(Forte),
            typeof(Reverb), typeof(CryingOutLoud), typeof(Encore),
            typeof(Muffle), typeof(Feedback), typeof(SingAlong), typeof(Silence),
        },
        ["damage"] = new[]
        {
            typeof(Crash), typeof(DesperateStrike), typeof(RunThrough),
            typeof(FreezeUp), typeof(HeartAche), typeof(Pathos),
        },
        ["block"] = new[]
        {
            typeof(FreezeUp), typeof(Foreshadow), typeof(TheWall),
            typeof(Composure), typeof(SilverLining), typeof(Muffle), typeof(DeadWeight),
        },
        ["draw"] = new[]
        {
            typeof(Orchestration), typeof(Signature),
            typeof(TakeNotes), typeof(Cram),
        },
        ["energy"] = new[]
        {
            typeof(Forte), typeof(BurnOut), typeof(NervousEnergy),
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
    [InlineData("Common", "Swap")]
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
    [InlineData("Uncommon", "Swap")]
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
    [InlineData("Rare", "Swap")]
    public void Rarity_Represents_Mechanic(string rarity, string mechanic)
    {
        var want = Enum.Parse<CardRarity>(rarity);
        Assert.True(
            MechanicCards[mechanic].Any(t => RarityOf(t) == want),
            $"No {rarity} card embodies '{mechanic}'.");
    }
}
