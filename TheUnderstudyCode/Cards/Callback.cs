using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
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

public class Callback : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Callback";

    public Callback() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(9);
        WithVars(new CardsVar("Select", 1));
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var player = cardPlay.Card.Owner;
        var selected = await ExactPileSelection.Select(
            context, PileType.Discard.GetPile(player), player, (int)DynamicVars["Select"].BaseValue,
            "THEUNDERSTUDY-CALLBACK.selectionPrompt",
            c => c != this && PlannedModifier.CanApplyTo(c), armPlannedBadges: true);

        foreach (var card in PlannedSelectionState.InClickOrder(selected.ToList()))
            PlannedModifier.Apply(card, CombatState!);
    }
}
