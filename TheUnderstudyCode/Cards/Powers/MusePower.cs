using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Patches;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

public class MusePower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "Muse",
        "At the start of your turn, apply [gold]Planned[/gold] to this many cards.",
        "At the start of your turn, apply [gold]Planned[/gold] to [blue]{Amount}[/blue] {Amount:plural:card|cards}.");

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        int maxSelect = (int)Amount;
        PlannedSelectionState.Arm();
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-MUSE.selectionPrompt"), 0, maxSelect),
            PlannedModifier.CanApplyTo,
            this);
        if (selected == null) return;
        foreach (var card in PlannedSelectionState.OrderFor(selected))
            PlannedModifier.Apply(card, Owner.CombatState!);
    }
}
