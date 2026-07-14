using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class MuscleMemory : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:MuscleMemory";

    public MuscleMemory() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithPowerNoTip<MuscleMemoryPower>(1, 1);
        WithTip(UnderstudyKeywords.Tuned);
        WithTip(CardKeyword.Unplayable);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<MuscleMemoryPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
