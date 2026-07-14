using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Experience : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Experience";

    public Experience() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove);
        WithTip(UnderstudyKeywords.Tuned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        foreach (var card in PlannedModifier.RelevantCards(player))
            TunedModifier.DoubleStacks(card);
        await Task.CompletedTask;
    }
}
