using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Bare-instantiation smoke tests for the 45-card deck redesign — mirrors the constraint documented
// in UnderstudyCardTests.cs (no ModelDb available), so these only assert on properties settable
// directly from the ConstructedCardModel constructor: CardId, Type, Rarity, TargetType.
public class NewDeckCardsTests
{
    [Theory]
    [InlineData(typeof(FreezeUp), "TheUnderstudy:FreezeUp", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(DesperateStrike), "TheUnderstudy:DesperateStrike", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(MoveOn), "TheUnderstudy:MoveOn", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(PracticeStrike), "TheUnderstudy:PracticeStrike", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(Development), "TheUnderstudy:Development", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Rehearse), "TheUnderstudy:Rehearse", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Innovation), "TheUnderstudy:Innovation", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Blackout), "TheUnderstudy:Blackout", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(AllNighter), "TheUnderstudy:AllNighter", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(MustGoOn), "TheUnderstudy:MustGoOn", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(MissedCue), "TheUnderstudy:MissedCue", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(StageFright), "TheUnderstudy:StageFright", CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)]
    [InlineData(typeof(Breather), "TheUnderstudy:Breather", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(TakeTwo), "TheUnderstudy:TakeTwo", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(BuyTime), "TheUnderstudy:BuyTime", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(CryingOutLoud), "TheUnderstudy:CryingOutLoud", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Preview), "TheUnderstudy:Preview", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Balanced), "TheUnderstudy:Balanced", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Arrangement), "TheUnderstudy:Arrangement", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(OneUp), "TheUnderstudy:OneUp", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(DaCapo), "TheUnderstudy:DaCapo", CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(WindUp), "TheUnderstudy:WindUp", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(DoubleTime), "TheUnderstudy:DoubleTime", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(LoosenUp), "TheUnderstudy:LoosenUp", CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)]
    [InlineData(typeof(HaveFun), "TheUnderstudy:HaveFun", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(StrikeAPose), "TheUnderstudy:StrikeAPose", CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)]
    [InlineData(typeof(FolkSong), "TheUnderstudy:FolkSong", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(LoveSong), "TheUnderstudy:LoveSong", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(SadSong), "TheUnderstudy:SadSong", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(PopSong), "TheUnderstudy:PopSong", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(OldSong), "TheUnderstudy:OldSong", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(CenterStage), "TheUnderstudy:CenterStage", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(MasterForm), "TheUnderstudy:MasterForm", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(HeldNote), "TheUnderstudy:HeldNote", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(Showtime), "TheUnderstudy:Showtime", CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(SellOut), "TheUnderstudy:SellOut", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(OwnIt), "TheUnderstudy:OwnIt", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(SonicBoom), "TheUnderstudy:SonicBoom", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(TheFirstLesson), "TheUnderstudy:TheFirstLesson", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(AnotherBrick), "TheUnderstudy:AnotherBrick", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Showstopper), "TheUnderstudy:Showstopper", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(Medley), "TheUnderstudy:Medley", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(HeartAche), "TheUnderstudy:HeartAche", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(DrawingBlanks), "TheUnderstudy:DrawingBlanks", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(StartOver), "TheUnderstudy:StartOver", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(MagnumOpus), "TheUnderstudy:MagnumOpus", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(Motif), "TheUnderstudy:Motif", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(AllHands), "TheUnderstudy:AllHands", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(Memorize), "TheUnderstudy:Memorize", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Forte), "TheUnderstudy:Forte", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Crash), "TheUnderstudy:Crash", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(Subversion), "TheUnderstudy:Subversion", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Joke), "TheUnderstudy:Joke", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(RunningOnFumes), "TheUnderstudy:RunningOnFumes", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(Playlist), "TheUnderstudy:Playlist", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(Confidence), "TheUnderstudy:Confidence", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Muse), "TheUnderstudy:Muse", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Schedule), "TheUnderstudy:Schedule", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(MuscleMemory), "TheUnderstudy:MuscleMemory", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(Improvise), "TheUnderstudy:Improvise", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Canonical), "TheUnderstudy:Canonical", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(CleanSlate), "TheUnderstudy:CleanSlate", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(Experience), "TheUnderstudy:Experience", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(SecondNature), "TheUnderstudy:SecondNature", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(Apathy), "TheUnderstudy:Apathy", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(OneTake), "TheUnderstudy:OneTake", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(Pathos), "TheUnderstudy:Pathos", CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)]
    [InlineData(typeof(CribNotes), "TheUnderstudy:CribNotes", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(BrightSide), "TheUnderstudy:BrightSide", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Crescendo), "TheUnderstudy:Crescendo", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Venue), "TheUnderstudy:Venue", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(TheWall), "TheUnderstudy:TheWall", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(TuningRitual), "TheUnderstudy:TuningRitual", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Signature), "TheUnderstudy:Signature", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(AutoTune), "TheUnderstudy:AutoTune", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Fate), "TheUnderstudy:Fate", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    public void Card_ConstructsWithExpectedShape(
        System.Type cardType, string expectedId, CardType expectedType, CardRarity expectedRarity, TargetType expectedTarget)
    {
        var card = (UnderstudyCard)System.Activator.CreateInstance(cardType)!;
        var idField = cardType.GetField("CardId");
        Assert.NotNull(idField);
        Assert.Equal(expectedId, idField!.GetValue(null));
        Assert.Equal(expectedType, card.Type);
        Assert.Equal(expectedRarity, card.Rarity);
        Assert.Equal(expectedTarget, card.TargetType);
    }

    // StartOver / Showstopper: converted to start-Tuned-1 (Part C of the Tuned-generalization
    // plan). StartOver drops Exhaust in favor of a cost-reduction upgrade; Showstopper's damage is
    // raised to compensate for losing "always replayable."

    [Fact]
    public void MissedCue_IsPreTuned() => Assert.True(new StartOver().IsPreTuned);

    [Fact]
    public void MissedCue_HasNoExhaustKeyword() =>
        Assert.False(new StartOver().Keywords.Contains(CardKeyword.Exhaust));

    [Fact]
    public void Showstopper_IsPreTuned() => Assert.True(new Showstopper().IsPreTuned);

    [Fact]
    public void Showstopper_DamageIs34() =>
        Assert.Equal(34m, new Showstopper().DynamicVars.Damage.BaseValue);

    [Fact]
    public void FinalDraft_HasExhaustKeyword() =>
        Assert.True(new Canonical().Keywords.Contains(CardKeyword.Exhaust));

    [Fact]
    public void CleanSlate_IsPreTuned() => Assert.True(new CleanSlate().IsPreTuned);

    [Fact]
    public void CleanSlate_DamageIs4() =>
        Assert.Equal(4m, new CleanSlate().DynamicVars.Damage.BaseValue);

    [Fact]
    public void CutTheTension_HasExhaustKeyword() =>
        Assert.True(new Experience().Keywords.Contains(CardKeyword.Exhaust));

    [Fact]
    public void SecondNature_DamageIs24() =>
        Assert.Equal(24m, new SecondNature().DynamicVars.Damage.BaseValue);

    [Fact]
    public void SecondNature_HasNoTunedModifierByDefault() =>
        Assert.False(new SecondNature().TryGetModifier<TunedModifier>(out _));
}
