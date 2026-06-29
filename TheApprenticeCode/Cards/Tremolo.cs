using TheApprentice.TheApprenticeCode.Cards.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Tremolo : ApprenticeCard
{
    public const string CardId = "TheApprentice:Tremolo";

    public Tremolo() : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithBlock(5);
        WithTip(typeof(WeakPower));
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
        int weakStacks = IsUpgraded ? 3 : 2;
        await PowerCmd.Apply<WeakPower>(context, cardPlay.Target!, weakStacks, cardPlay.Card.Owner.Creature, cardPlay.Card, false);
        await TensionHelper.AddTension(context, cardPlay.Card.Owner.Creature, 3, cardPlay.Card);
    }
}
