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

public class Foreshadow : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Foreshadow";

    public Foreshadow() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(8);
        WithVars(new CardsVar("Select", 1));
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);

        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        var pile = PileType.Draw.GetPile(player);
        PlannedSelectionState.Arm();
        var selected = await CardSelectCmd.FromCombatPile(
            context,
            pile,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-FORESHADOW.selectionPrompt"), 0, maxSelect),
            c => PlannedModifier.CanApplyTo(c));

        foreach (var card in PlannedSelectionState.OrderFor(selected))
            PlannedModifier.Apply(card, CombatState!);
    }
}
