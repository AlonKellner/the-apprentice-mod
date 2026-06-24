using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Obsession : ApprenticeCard
{
    public const string CardId = "TheApprentice:Obsession";

    public Obsession() : base(1, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithBlock(2);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = (Creature)(object)cardPlay.Card.Owner;
        await PowerCmd.Apply(context, new ObsessionPower(), creature, IsUpgraded ? 3m : 2m, creature, cardPlay.Card, false);
    }
}
