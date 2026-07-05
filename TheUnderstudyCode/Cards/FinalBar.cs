using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class FinalBar : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:FinalBar";

    public FinalBar() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithVars(
            new CalculationBaseVar(0m),
            new ExtraDamageVar(5m).WithUpgrade(2m),
            new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) =>
                PlannedModifier.CountIn(PlannedModifier.RelevantCards(card.Owner))));
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override bool ShouldGlowGoldInternal => PlannedModifier.AnyIn(PlannedModifier.RelevantCards(Owner));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        // Snapshot the same count CalculatedDamageVar's multiplier lambda used to scale this
        // attack's damage. The attack itself doesn't mutate Planned state, so the strip loop
        // below must remove exactly this many cards.
        int damageScalingCount = PlannedModifier.CountIn(PlannedModifier.RelevantCards(player));

        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var allCardsList = PlannedModifier.RelevantCards(player).ToList();
        var plannedCards = allCardsList.Where(c => c.TryGetModifier<PlannedModifier>(out _)).ToList();
        Log.Info($"FinalBar.OnPlay: stripping Planned from {plannedCards.Count} card(s) without playing them");
        Invariants.CheckEqual(damageScalingCount, plannedCards.Count, nameof(FinalBar) + "." + nameof(OnPlay),
            "Planned count used for damage scaling vs. Planned cards stripped afterward");
        foreach (var card in plannedCards)
        {
            PlannedModifier.Remove(card, allCardsList);
            if (card.Pile?.Type is PileType.Hand or PileType.Draw or PileType.Discard)
                await CardCmd.Exhaust(context, card);
        }
    }
}
