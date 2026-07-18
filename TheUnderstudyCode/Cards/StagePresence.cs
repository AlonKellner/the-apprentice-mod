using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// A commanding presence: every turn, the room's problems become theirs. (Swap = Audience / Interaction.)
public class StagePresence : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StagePresence";

    public StagePresence() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithPowerNoTip<StagePresencePower>(2);
        WithTip(UnderstudyKeywords.Swap);
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<StagePresencePower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
