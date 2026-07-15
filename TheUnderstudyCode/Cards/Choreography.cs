using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Patches;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Choreography : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Choreography";

    public Choreography() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVars(new IntVar("Invert", 2));
        WithTip(UnderstudyKeywords.Invert);
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Invert"].UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int invertAmount = (int)DynamicVars["Invert"].BaseValue;
        await EmotionalExpression.InvertLastModified(context, cardPlay.Card.Owner.Creature, invertAmount);

        var player = cardPlay.Card.Owner;
        PlannedSelectionState.Arm();
        var selected = await CardSelectCmd.FromCombatPile(
            context,
            PileType.Discard.GetPile(player),
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-CHOREOGRAPHY.selectionPrompt"), 0, 1),
            c => c != this && PlannedModifier.CanApplyTo(c));

        if (selected == null) return;
        foreach (var card in PlannedSelectionState.OrderFor(selected))
            PlannedModifier.Apply(card, CombatState!);
    }
}
