using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TableRead : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TableRead";

    public TableRead() : base(4, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithTip(UnderstudyKeywords.Planned);
        WithTip(typeof(UnweakPower));
        WithTip(typeof(UnvulnerablePower));
        WithTip(typeof(UnshakenPower));
        WithTip(typeof(UnlimitedPower));
        WithTip(typeof(UnjadedPower));
    }

    public override bool IsPrePlanned => true;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<UnweakPower>(context, creature, 1, creature, cardPlay.Card, false);
        await PowerCmd.Apply<UnvulnerablePower>(context, creature, 1, creature, cardPlay.Card, false);
        await PowerCmd.Apply<UnshakenPower>(context, creature, 1, creature, cardPlay.Card, false);
        await PowerCmd.Apply<UnlimitedPower>(context, creature, 1, creature, cardPlay.Card, false);
        await PowerCmd.Apply<UnjadedPower>(context, creature, 1, creature, cardPlay.Card, false);
    }
}
