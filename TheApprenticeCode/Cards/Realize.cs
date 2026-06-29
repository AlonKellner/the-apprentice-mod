using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Realize : ApprenticeCard
{
    public const string CardId = "TheApprentice:Realize";

    public Realize() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
        WithDamage(8);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var selected = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-REALIZE.selectionPrompt"), 0, 1),
            c => c != cardPlay.Card && PlannedModifier.CanApplyTo(c),
            this);

        var target = selected?.FirstOrDefault();
        if (target != null)
            PlannedModifier.Apply(target, player.Piles.SelectMany(p => p.Cards));
    }
}
