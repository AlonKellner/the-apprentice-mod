using MegaCrit.Sts2.Core.Entities.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards;
using Xunit;

namespace TheUnderstudy.Tests.Cards;

// Bare-instantiation smoke tests for the 45-card deck redesign — mirrors the constraint documented
// in UnderstudyCardTests.cs (no ModelDb available), so these only assert on properties settable
// directly from the ConstructedCardModel constructor: CardId, Type, Rarity, TargetType.
public class NewDeckCardsTests
{
    [Theory]
    [InlineData(typeof(FreezeUp), "TheUnderstudy:FreezeUp", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(StageWhisper), "TheUnderstudy:StageWhisper", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(RawNerve), "TheUnderstudy:RawNerve", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(Unguarded), "TheUnderstudy:Unguarded", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(TakeABreath), "TheUnderstudy:TakeABreath", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(Reprise), "TheUnderstudy:Reprise", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(TellItLikeItIs), "TheUnderstudy:TellItLikeItIs", CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(BareItAll), "TheUnderstudy:BareItAll", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(Cue), "TheUnderstudy:Cue", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Rehearse), "TheUnderstudy:Rehearse", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(CramSession), "TheUnderstudy:CramSession", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Overexert), "TheUnderstudy:Overexert", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(QuickNap), "TheUnderstudy:QuickNap", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(AllNighter), "TheUnderstudy:AllNighter", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(OpeningNumber), "TheUnderstudy:OpeningNumber", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(TakeCenterStage), "TheUnderstudy:TakeCenterStage", CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)]
    [InlineData(typeof(Rewrite), "TheUnderstudy:Rewrite", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(TakeTwo), "TheUnderstudy:TakeTwo", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Flourish), "TheUnderstudy:Flourish", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(TakeNotes), "TheUnderstudy:TakeNotes", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Foreshadow), "TheUnderstudy:Foreshadow", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(StandingBy), "TheUnderstudy:StandingBy", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(Arrangement), "TheUnderstudy:Arrangement", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(DaCapo), "TheUnderstudy:DaCapo", CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(Encore), "TheUnderstudy:Encore", CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)]
    [InlineData(typeof(Diminuendo), "TheUnderstudy:Diminuendo", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Fortissimo), "TheUnderstudy:Fortissimo", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(TakeYourBow), "TheUnderstudy:TakeYourBow", CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)]
    [InlineData(typeof(SteadyNow), "TheUnderstudy:SteadyNow", CardType.Skill, CardRarity.Common, TargetType.None)]
    [InlineData(typeof(Coda), "TheUnderstudy:Coda", CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)]
    [InlineData(typeof(FolkSong), "TheUnderstudy:FolkSong", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(LoveSong), "TheUnderstudy:LoveSong", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(SadSong), "TheUnderstudy:SadSong", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(PopSong), "TheUnderstudy:PopSong", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(OldSong), "TheUnderstudy:OldSong", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(DressRehearsal), "TheUnderstudy:DressRehearsal", CardType.Skill, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(MasterForm), "TheUnderstudy:MasterForm", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(HeldNote), "TheUnderstudy:HeldNote", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(CurtainCall), "TheUnderstudy:CurtainCall", CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(FinalBar), "TheUnderstudy:FinalBar", CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)]
    [InlineData(typeof(EverythingIveGot), "TheUnderstudy:EverythingIveGot", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(BigBreak), "TheUnderstudy:BigBreak", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(TouchUp), "TheUnderstudy:TouchUp", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(WideOpen), "TheUnderstudy:WideOpen", CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)]
    [InlineData(typeof(TheFirstLesson), "TheUnderstudy:TheFirstLesson", CardType.Power, CardRarity.Rare, TargetType.None)]
    [InlineData(typeof(StandingRoom), "TheUnderstudy:StandingRoom", CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)]
    [InlineData(typeof(HouseLights), "TheUnderstudy:HouseLights", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(FullVoice), "TheUnderstudy:FullVoice", CardType.Power, CardRarity.Uncommon, TargetType.None)]
    [InlineData(typeof(NightShift), "TheUnderstudy:NightShift", CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)]
    [InlineData(typeof(Remix), "TheUnderstudy:Remix", CardType.Skill, CardRarity.Uncommon, TargetType.None)]
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
}
