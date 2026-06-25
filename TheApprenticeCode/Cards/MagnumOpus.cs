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

public class MagnumOpus : ApprenticeCard
{
    public const string CardId = "TheApprentice:MagnumOpus";

    public MagnumOpus() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithCards(3);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int maxCards = IsUpgraded ? 4 : 3;

        var selected = await MultiPileCardSelect.Select(
            context, player,
            new CardSelectorPrefs(
                new LocString("cards", "THEAPPRENTICE-MAGNUM_OPUS.selectionPrompt"), 1, maxCards),
            c => PlannedModifier.CanApplyTo(c),
            PileType.Draw);

        if (selected == null) return;
        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in selected)
            PlannedModifier.Apply(card, allCards);
    }
}
