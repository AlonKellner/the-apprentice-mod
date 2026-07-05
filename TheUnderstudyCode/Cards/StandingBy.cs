using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class StandingBy : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StandingBy";

    public StandingBy() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithPower<StandingByPower>(1, 1);
        WithTip(CardKeyword.Unplayable);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        var power = await CommonActions.Apply<StandingByPower>(context, creature, this);
        Invariants.Check(power != null, nameof(StandingBy) + "." + nameof(OnPlay),
            "StandingByPower must exist immediately after Apply — an upgraded copy's ChoiceMode would be silently dropped");
        power?.ResetTracking();
        if (cardPlay.Card.IsUpgraded && power != null) power.ChoiceMode = true;
    }
}
