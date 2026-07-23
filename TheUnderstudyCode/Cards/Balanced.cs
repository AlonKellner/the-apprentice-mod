using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Balanced : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Balanced";

    public Balanced() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(CardKeyword.Unplayable);
        WithCostUpgradeBy(-1); // upgrade: cost 2 -> 1
    }

    // One power either way — upgrading makes the card cheaper rather than changing how the freed
    // cards are picked, so there is no player-choice variant. Amount 1 per play (explicit-amount
    // Apply overload); each stack frees one card per trigger and the power reads its own Amount.
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        var power = await CommonActions.Apply<BalancedPower>(context, creature, this, 1);
        Invariants.Check(power != null, nameof(Balanced) + "." + nameof(OnPlay),
            "BalancedPower must exist immediately after Apply");
        power?.ResetTracking();
    }
}
