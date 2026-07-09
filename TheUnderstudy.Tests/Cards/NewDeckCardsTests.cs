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
    [InlineData(typeof(StageWhisper), "TheUnderstudy:StageWhisper", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(TakeABreath), "TheUnderstudy:TakeABreath", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Reprise), "TheUnderstudy:Reprise", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(Cue), "TheUnderstudy:Cue", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Rehearse), "TheUnderstudy:Rehearse", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(CramSession), "TheUnderstudy:CramSession", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Overexert), "TheUnderstudy:Overexert", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(QuickNap), "TheUnderstudy:QuickNap", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(AllNighter), "TheUnderstudy:AllNighter", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(OpeningNumber), "TheUnderstudy:OpeningNumber", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(TakeCenterStage), "TheUnderstudy:TakeCenterStage", CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)]
    [InlineData(typeof(Rewrite), "TheUnderstudy:Rewrite", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(TakeTwo), "TheUnderstudy:TakeTwo", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Flourish), "TheUnderstudy:Flourish", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(TakeNotes), "TheUnderstudy:TakeNotes", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Foreshadow), "TheUnderstudy:Foreshadow", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(StandingBy), "TheUnderstudy:StandingBy", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Arrangement), "TheUnderstudy:Arrangement", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(DaCapo), "TheUnderstudy:DaCapo", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(Encore), "TheUnderstudy:Encore", CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(WindUp), "TheUnderstudy:WindUp", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(DoubleTime), "TheUnderstudy:DoubleTime", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(TakeYourBow), "TheUnderstudy:TakeYourBow", CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)]
    [InlineData(typeof(SteadyNow), "TheUnderstudy:SteadyNow", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Coda), "TheUnderstudy:Coda", CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)]
    [InlineData(typeof(FolkSong), "TheUnderstudy:FolkSong", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(LoveSong), "TheUnderstudy:LoveSong", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(SadSong), "TheUnderstudy:SadSong", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(PopSong), "TheUnderstudy:PopSong", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(OldSong), "TheUnderstudy:OldSong", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(DressRehearsal), "TheUnderstudy:DressRehearsal", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(MasterForm), "TheUnderstudy:MasterForm", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(HeldNote), "TheUnderstudy:HeldNote", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(CurtainCall), "TheUnderstudy:CurtainCall", CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(FinalBar), "TheUnderstudy:FinalBar", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(EverythingIveGot), "TheUnderstudy:EverythingIveGot", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(BigBreak), "TheUnderstudy:BigBreak", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(TouchUp), "TheUnderstudy:TouchUp", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(WideOpen), "TheUnderstudy:WideOpen", CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)]
    [InlineData(typeof(TheFirstLesson), "TheUnderstudy:TheFirstLesson", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(FullVoice), "TheUnderstudy:FullVoice", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Showstopper), "TheUnderstudy:Showstopper", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(Remix), "TheUnderstudy:Remix", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Overcommit), "TheUnderstudy:Overcommit", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(FastForward), "TheUnderstudy:FastForward", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(MissedCue), "TheUnderstudy:MissedCue", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(Prompt), "TheUnderstudy:Prompt", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(MagnumOpus), "TheUnderstudy:MagnumOpus", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(Refrain), "TheUnderstudy:Refrain", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(NervousEnergy), "TheUnderstudy:NervousEnergy", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(StageFright), "TheUnderstudy:StageFright", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(BreakALeg), "TheUnderstudy:BreakALeg", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Bravado), "TheUnderstudy:Bravado", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(StandingOvation), "TheUnderstudy:StandingOvation", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(PlotTwist), "TheUnderstudy:PlotTwist", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(TrueColors), "TheUnderstudy:TrueColors", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Matinee), "TheUnderstudy:Matinee", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(TableRead), "TheUnderstudy:TableRead", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(SafetyNet), "TheUnderstudy:SafetyNet", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(CallSheet), "TheUnderstudy:CallSheet", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(WarmUp), "TheUnderstudy:WarmUp", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(MuscleMemory), "TheUnderstudy:MuscleMemory", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(Improvise), "TheUnderstudy:Improvise", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(FinalDraft), "TheUnderstudy:FinalDraft", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(CleanSlate), "TheUnderstudy:CleanSlate", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(CutTheTension), "TheUnderstudy:CutTheTension", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(SecondNature), "TheUnderstudy:SecondNature", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(PulledPunch), "TheUnderstudy:PulledPunch", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(OneTake), "TheUnderstudy:OneTake", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(Ensemble), "TheUnderstudy:Ensemble", CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)]
    [InlineData(typeof(OffScript), "TheUnderstudy:OffScript", CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)]
    [InlineData(typeof(AdLib), "TheUnderstudy:AdLib", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Crescendo), "TheUnderstudy:Crescendo", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(StageManager), "TheUnderstudy:StageManager", CardType.Power, CardRarity.Rare, TargetType.None)]
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

    // MissedCue / Showstopper: converted to start-Intense-1 (Part C of the Intense-generalization
    // plan). MissedCue drops Exhaust in favor of a cost-reduction upgrade; Showstopper's damage is
    // raised to compensate for losing "always replayable."

    [Fact]
    public void MissedCue_IsPreIntense() => Assert.True(new MissedCue().IsPreIntense);

    [Fact]
    public void MissedCue_HasNoExhaustKeyword() =>
        Assert.False(new MissedCue().Keywords.Contains(CardKeyword.Exhaust));

    [Fact]
    public void Showstopper_IsPreIntense() => Assert.True(new Showstopper().IsPreIntense);

    [Fact]
    public void Showstopper_DamageIs34() =>
        Assert.Equal(34m, new Showstopper().DynamicVars.Damage.BaseValue);

    [Fact]
    public void FinalDraft_HasExhaustKeyword() =>
        Assert.True(new FinalDraft().Keywords.Contains(CardKeyword.Exhaust));

    [Fact]
    public void CleanSlate_IsPreIntense() => Assert.True(new CleanSlate().IsPreIntense);

    [Fact]
    public void CleanSlate_DamageIs4() =>
        Assert.Equal(4m, new CleanSlate().DynamicVars.Damage.BaseValue);

    [Fact]
    public void CutTheTension_HasExhaustKeyword() =>
        Assert.True(new CutTheTension().Keywords.Contains(CardKeyword.Exhaust));

    [Fact]
    public void SecondNature_DamageIs8() =>
        Assert.Equal(8m, new SecondNature().DynamicVars.Damage.BaseValue);

    [Fact]
    public void SecondNature_HasNoIntenseModifierByDefault() =>
        Assert.False(new SecondNature().TryGetModifier<IntenseModifier>(out _));
}
