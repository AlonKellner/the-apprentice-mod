using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Plan : ApprenticeCard
{
    public const string CardId = "TheApprentice:Plan";

    public Plan() : base(0, CardType.Skill, CardRarity.Basic, TargetType.None, false)
    {
        WithCards(1);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        await CommonActions.Draw(cardPlay.Card, context);

        var maxSelect = IsUpgraded ? 3 : 2;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-PLAN.selectionPrompt"), 1, maxSelect),
            c => c != this && !c.TryGetModifier<PlannedModifier>(out _),
            this);

        if (selected == null) return;
        foreach (var card in selected)
            PlannedModifier.Apply(card, player.Piles.SelectMany(p => p.Cards));
    }
}
