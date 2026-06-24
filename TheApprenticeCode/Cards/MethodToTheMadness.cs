using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class MethodToTheMadness : ApprenticeCard
{
    public const string CardId = "TheApprentice:MethodToTheMadness";

    public MethodToTheMadness() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = (Creature)(object)cardPlay.Card.Owner;
        await PowerCmd.Apply(context, new MethodToTheMadnessPower(), creature, IsUpgraded ? 1m : 0m, creature, cardPlay.Card, false);
    }
}
