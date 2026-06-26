using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Ignition : ApprenticeCard
{
    public const string CardId = "TheApprentice:Ignition";

    public Ignition() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(2);
        WithTip(typeof(Potential));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await DreamsAndAmbitions.AddPotentials(player, CombatState!, IsUpgraded ? 3 : 2, false);
    }
}
