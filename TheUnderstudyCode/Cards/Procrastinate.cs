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

public class Procrastinate : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Procrastinate";

    public Procrastinate() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(13);
        WithVars(new IntVar("Swap", 3));
        WithTip(UnderstudyKeywords.Planned);
        WithTip(UnderstudyKeywords.Swap);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var player = cardPlay.Card.Owner;
        PlannedSelectionState.Arm();
        var selected = await CardSelectCmd.FromCombatPile(
            context,
            PileType.Draw.GetPile(player),
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-PROCRASTINATE.selectionPrompt"), 0, 1),
            c => c != this && PlannedModifier.CanApplyTo(c));
        if (selected != null)
            foreach (var card in PlannedSelectionState.OrderFor(selected))
                PlannedModifier.Apply(card, CombatState!);

        await SceneStealing.SwapEach(context, cardPlay.Card.Owner.Creature, (int)DynamicVars["Swap"].BaseValue);
    }
}
