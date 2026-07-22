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
using TheUnderstudy.TheUnderstudyCode.Patches;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class WriteItDown : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:WriteItDown";

    public WriteItDown() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVars(new CardsVar("Select", 1));
        WithTip(UnderstudyKeywords.Planned);
        WithTip(UnderstudyKeywords.Tuned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        PlannedSelectionState.Arm();
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-WRITE_IT_DOWN.selectionPrompt"), 0, maxSelect),
            c => c != this && PlannedModifier.CanApplyTo(c) && TunedModifier.CanApplyTo(c),
            this);
        if (selected == null) return;

        foreach (var card in PlannedSelectionState.OrderFor(selected))
        {
            PlannedModifier.Apply(card, CombatState!);
            TunedModifier.Apply(card);
        }
    }
}
