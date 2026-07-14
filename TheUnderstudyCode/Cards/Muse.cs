using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Muse : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Muse";

    public Muse() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithPowerNoTip<MusePower>(1);
        WithTip(UnderstudyKeywords.Planned);
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<MusePower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
