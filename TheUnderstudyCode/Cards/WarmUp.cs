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

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class WarmUp : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:WarmUp";

    public WarmUp() : base(0, CardType.Skill, CardRarity.Basic, TargetType.None, false)
    {
        WithKeyword(UnderstudyKeywords.Stable, ConstructedCardModel.UpgradeType.None);
        WithKeyword(CardKeyword.Retain, ConstructedCardModel.UpgradeType.None);
        WithVars(new CardsVar("Select", 2));
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
        var maxSelect = IsUpgraded ? 3 : 2;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-WARM_UP.selectionPrompt"), 0, maxSelect),
            c => c != this && TunedModifier.CanApplyTo(c),
            this);

        if (selected == null) return;
        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in selected)
            TunedModifier.Apply(card, CombatState!, allCards);
    }
}
