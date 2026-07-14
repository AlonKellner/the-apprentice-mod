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

    public Balanced() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(CardKeyword.Unplayable);
    }

    // Base applies the random BalancedPower; upgraded applies the choice BalancedChoicePower.
    // They're distinct power types, so playing one of each leaves two independent Counter badges,
    // and replaying either stacks only its own kind. Amount 1 per play (explicit-amount Apply
    // overload) — each stack frees one card per trigger; the power reads its own Amount.
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        if (cardPlay.Card.IsUpgraded)
        {
            var power = await CommonActions.Apply<BalancedChoicePower>(context, creature, this, 1);
            Invariants.Check(power != null, nameof(Balanced) + "." + nameof(OnPlay),
                "BalancedChoicePower must exist immediately after Apply");
            power?.ResetTracking();
        }
        else
        {
            var power = await CommonActions.Apply<BalancedPower>(context, creature, this, 1);
            Invariants.Check(power != null, nameof(Balanced) + "." + nameof(OnPlay),
                "BalancedPower must exist immediately after Apply");
            power?.ResetTracking();
        }
    }
}
