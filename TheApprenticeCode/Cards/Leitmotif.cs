using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Commands;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Leitmotif : ApprenticeCard
{
    public const string CardId = "TheApprentice:Leitmotif";

    public Leitmotif() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(8);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var selected = await MultiPileCardSelect.Select(
            context, player,
            new CardSelectorPrefs(
                new LocString("cards", "THEAPPRENTICE-LEITMOTIF.selectionPrompt"), 0, 1),
            c => PlannedModifier.CanApplyTo(c),
            PileType.Discard);

        if (selected == null) return;
        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in selected)
            PlannedModifier.Apply(card, allCards);
    }
}
