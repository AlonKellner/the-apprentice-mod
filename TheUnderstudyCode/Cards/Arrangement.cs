using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Patches;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Arrangement : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Arrangement";

    public Arrangement() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(9);
        WithTip(UnderstudyKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var player = cardPlay.Card.Owner;
        PlannedSelectionState.Arm();
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-ARRANGEMENT.selectionPrompt"), 0, 2),
            c => c != this && PlannedModifier.CanApplyTo(c),
            this);

        if (selected == null) return;
        foreach (var card in PlannedSelectionState.OrderFor(selected))
            PlannedModifier.Apply(card, CombatState!);
    }
}
