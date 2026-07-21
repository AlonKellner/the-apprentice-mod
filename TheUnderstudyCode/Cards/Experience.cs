using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Experience : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Experience";

    public Experience() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithCostUpgradeBy(-1); // upgrade: cost 3 -> 2
        // Base 0 on purpose: the card is always pre-Tuned and its damage is purely its Tuned bonus, so the
        // dynamic base damage shown/dealt equals the total Tuned across the deck.
        WithDamage(0);
        WithTip(UnderstudyKeywords.Tuned);
    }

    public override bool IsPreTuned => true;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        var player = cardPlay.Card.Owner;
        foreach (var card in PlannedModifier.RelevantCards(player))
            TunedModifier.DoubleStacks(card);
    }
}
