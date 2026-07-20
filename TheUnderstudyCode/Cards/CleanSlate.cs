using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class CleanSlate : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:CleanSlate";

    public CleanSlate() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(3);
        // Live "(Hits N times)" preview: N = number of Unplayable cards this will Exhaust and hit
        // (CalculationBase 0 + CalculationExtra 1 * count). Mirrors base-game FlakCannon.
        WithVars(
            new CalculationBaseVar(0m),
            new CalculationExtraVar(1m),
            new CalculatedVar("CalculatedHits").WithMultiplier(static (card, _) => ExhaustTargets(card).Count));
        WithTip(CardKeyword.Unplayable);
        WithTip(UnderstudyKeywords.Tuned);
    }

    // The cards this play will Exhaust and then hit once each: Unplayable cards in the Planned-relevant set,
    // excluding this card. Shared by the live preview var above and OnPlay so the shown count and the real
    // hit count can't drift. Static so the CalculatedVar multiplier delegate captures no instance.
    private static IReadOnlyList<CardModel> ExhaustTargets(CardModel card) =>
        PlannedModifier.RelevantCards(card.Owner).Where(c => c != card && c.IsUnplayable()).ToList();

    public override bool IsPreTuned => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal =>
        CardExtensions.AnyUnplayable(PlannedModifier.RelevantCards(Owner).Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var targets = ExhaustTargets(cardPlay.Card);

        foreach (var card in targets)
            if (card.Pile?.Type.IsCombatPile() == true)
                await CardCmd.Exhaust(context, card);

        if (targets.Count > 0)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay, targets.Count).Execute(context);
    }
}
