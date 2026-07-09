using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TakeNotes : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TakeNotes";

    public TakeNotes() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithPowerNoTip<TakeNotesPower>(3, 1);
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<TakeNotesPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
