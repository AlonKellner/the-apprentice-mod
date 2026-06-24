using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class InTheZone : ApprenticeCard
{
    public const string CardId = "TheApprentice:InTheZone";

    public InTheZone() : base(2, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithEnergy(1);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Energy.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = (Creature)(object)cardPlay.Card.Owner;
        await PowerCmd.Apply(context, new InTheZonePower(), creature, IsUpgraded ? 2m : 1m, creature, cardPlay.Card, false);
    }
}
