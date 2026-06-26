using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Blueprint : ApprenticeCard
{
    public const string CardId = "TheApprentice:Blueprint";

    public Blueprint() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.None);
        WithTip(ApprenticeKeywords.Planned);
        WithTip(typeof(Dream));
        WithTip(typeof(Ambition));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int plannedCount = PlannedModifier.CountIn(player.Piles.Where(p => p.Type == PileType.Hand).SelectMany(p => p.Cards));
        await DreamsAndAmbitions.AddDreams(player, CombatState!, plannedCount, IsUpgraded);
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, plannedCount, IsUpgraded);
    }
}
