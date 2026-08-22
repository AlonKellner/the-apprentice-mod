using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Patches;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Orchestration : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Orchestration";

    public Orchestration() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(1);
        WithVars(new CardsVar("Select", 1));
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Draw(this, context);

        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        var selected = await PlannedSelection.FromHand(
            context, player, maxSelect, "THEUNDERSTUDY-ORCHESTRATION.selectionPrompt",
            c => c != this && PlannedModifier.CanApplyTo(c), this);

        foreach (var card in PlannedSelectionState.InClickOrder(selected.ToList()))
            PlannedModifier.Apply(card, CombatState!);
    }
}
