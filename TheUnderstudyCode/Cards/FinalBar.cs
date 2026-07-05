using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class FinalBar : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:FinalBar";

    public FinalBar() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(5);
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    protected override bool ShouldGlowGoldInternal => PlannedModifier.AnyIn(PlannedModifier.RelevantCards(Owner));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var allCardsList = PlannedModifier.RelevantCards(player).ToList();
        var plannedCards = allCardsList.Where(c => c.TryGetModifier<PlannedModifier>(out _)).ToList();

        // One hit per Planned card, rather than a single hit scaled by the count.
        if (plannedCards.Count > 0)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay, plannedCards.Count).Execute(context);

        // Exhausting does NOT strip PlannedModifier — a Planned card keeps its queue slot(s) even
        // while sitting in the exhaust pile (IsCombatPile covers Exhaust), so CurtainCall/
        // Performance/Encore can still find and play it later via PlannedModifier.RelevantCards.
        Log.Info($"FinalBar.OnPlay: exhausting {plannedCards.Count} Planned card(s), keeping their Planned status");
        foreach (var card in plannedCards)
            if (card.Pile?.Type is PileType.Hand or PileType.Draw or PileType.Discard)
                await CardCmd.Exhaust(context, card);
    }
}
