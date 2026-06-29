using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Ensemble : ApprenticeCard
{
    public const string CardId = "TheApprentice:Ensemble";

    public Ensemble() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(TensionPower));
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int uniqueDebuffs = creature.Powers
            .Where(p => p.Type == PowerType.Debuff)
            .DistinctBy(p => p.GetType())
            .Count();
        int amount = IsUpgraded ? 4 : 3;
        await TensionHelper.AddTension(context, creature, uniqueDebuffs * amount, cardPlay.Card);
        await PowerCmd.Apply<VigorPower>(context, creature, uniqueDebuffs * amount, creature, cardPlay.Card, false);
    }
}
