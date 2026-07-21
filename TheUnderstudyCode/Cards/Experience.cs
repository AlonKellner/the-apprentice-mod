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
        WithDamage(1);
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
