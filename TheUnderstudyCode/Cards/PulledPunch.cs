using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class PulledPunch : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:PulledPunch";

    public PulledPunch() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithPower<PulledPunchPower>(1, 1);
        WithTip(UnderstudyKeywords.Invertible);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<PulledPunchPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
