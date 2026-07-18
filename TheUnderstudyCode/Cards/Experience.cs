using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Experience : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Experience";

    public Experience() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(1);
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove);
        WithTip(UnderstudyKeywords.Tuned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        var player = cardPlay.Card.Owner;
        foreach (var card in PlannedModifier.RelevantCards(player))
            TunedModifier.DoubleStacks(card);
    }
}
