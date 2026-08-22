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

public class MagnumOpus : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:MagnumOpus";

    public MagnumOpus() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithVars(new CardsVar("Select", 3));
        WithTip(UnderstudyKeywords.Planned);
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
        var pile = PileType.Draw.GetPile(player);
        var selected = await PlannedSelection.FromPile(
            context, pile, player, maxSelect, "THEUNDERSTUDY-MAGNUM_OPUS.selectionPrompt",
            c => PlannedModifier.CanApplyTo(c));

        foreach (var card in PlannedSelectionState.InClickOrder(selected.ToList()))
            PlannedModifier.Apply(card, CombatState!);
    }
}
