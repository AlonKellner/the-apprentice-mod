using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class OneTake : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:OneTake";

    // Ancient rarity: One Take is the Understudy's build-defining engine power (global -1 cost with the
    // Unplayable drawback + its Balanced/Muscle Memory/Resourceful support package). Like base-game
    // ancient powers it leaves the normal reward pool and is granted only by Darv's Dusty Tome (always
    // upgraded); DustyTome auto-selects it as the pool's sole non-transcendence Ancient card.
    public OneTake() : base(3, CardType.Power, CardRarity.Ancient, TargetType.None)
    {
        WithPowerNoTip<OneTakePower>(1);
        WithTip(CardKeyword.Unplayable);
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<OneTakePower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
