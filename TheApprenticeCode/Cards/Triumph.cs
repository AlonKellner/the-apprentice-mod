using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Triumph : ApprenticeCard
{
    public const string CardId = "TheApprentice:Triumph";

    public Triumph() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(TensionPower));
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int removed = await TensionHelper.RemoveTension(context, creature, 15, cardPlay.Card);
        if (removed <= 0) return;
        decimal blockMultiplier = IsUpgraded ? 3m : 2m;
        await CreatureCmd.GainBlock(creature, removed * blockMultiplier, ValueProp.Unpowered, null);
        await PowerCmd.Apply<VigorPower>(context, creature, removed, creature, cardPlay.Card, false);
    }
}
