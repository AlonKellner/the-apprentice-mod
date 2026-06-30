using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Reflection : ApprenticeCard
{
    public const string CardId = "TheApprentice:Reflection";

    public Reflection() : base(2, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithKeyword(ApprenticeKeywords.Expend, ConstructedCardModel.UpgradeType.None);
        WithTip(typeof(WeakPower));
        WithTip(typeof(StrengthPower));
    }

    public override bool HasExpend => true;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int weakAmount = creature.GetPowerAmount<WeakPower>();
        if (weakAmount <= 0) return;

        int cap = IsUpgraded ? 5 : 3;
        int removed = Math.Min(weakAmount, cap);
        await PowerCmd.Apply<WeakPower>(context, creature, -removed, creature, cardPlay.Card, false);
        await PowerCmd.Apply<StrengthPower>(context, creature, removed, creature, cardPlay.Card, false);
    }
}
