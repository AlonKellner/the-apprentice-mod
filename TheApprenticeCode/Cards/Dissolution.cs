using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Dissolution : ApprenticeCard
{
    public const string CardId = "TheApprentice:Dissolution";

    public Dissolution() : base(1, CardType.Skill, CardRarity.Basic, TargetType.AllEnemies)
    {
        WithTip(typeof(TensionPower));
        WithTip(typeof(WeakPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int removed = await TensionHelper.RemoveAllTension(context, creature, cardPlay.Card);
        int weakToApply = IsUpgraded ? removed / 3 : removed / 5;
        if (weakToApply > 0)
            foreach (var enemy in CombatState!.HittableEnemies)
                await PowerCmd.Apply<WeakPower>(context, enemy, weakToApply, creature, cardPlay.Card, false);
    }
}
