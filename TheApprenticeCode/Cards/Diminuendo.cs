using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Diminuendo : ApprenticeCard
{
    public const string CardId = "TheApprentice:Diminuendo";

    public Diminuendo() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithCards(1);
        WithBlock(14);
        WithTip(typeof(StrengthPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<StrengthPower>(context, creature, -1m, creature, cardPlay.Card, false);
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
        await CommonActions.Draw(this, context);
    }
}
