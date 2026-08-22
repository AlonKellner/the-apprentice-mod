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

public class KeepInMind : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:KeepInMind";

    public KeepInMind() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(1); // draw 1 (2 upgraded)
        WithVars(new CardsVar("Select", 1));
        WithTip(UnderstudyKeywords.Planned);
        WithTip(UnderstudyKeywords.Tuned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);   // only the draw upgrades (1 -> 2)
        // Select stays 1: Planned + Tuned are always applied to a single card.
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Draw(this, context);

        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        if (MultiplayerUtil.IsLocalPlayer(player)) PlannedSelectionState.Arm();
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-KEEP_IN_MIND.selectionPrompt"),maxSelect),
            c => c != this && PlannedModifier.CanApplyTo(c) && TunedModifier.CanApplyTo(c),
            this);
        if (selected == null) return;

        foreach (var card in PlannedSelectionState.InClickOrder(selected.ToList()))
        {
            PlannedModifier.Apply(card, CombatState!);
            TunedModifier.Apply(card);
        }
    }
}
