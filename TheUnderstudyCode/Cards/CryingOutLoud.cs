using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class CryingOutLoud : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:CryingOutLoud";

    public CryingOutLoud() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithPowerNoTip<CryingOutLoudPower>(3, 1);
        WithMarkedTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<CryingOutLoudPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
