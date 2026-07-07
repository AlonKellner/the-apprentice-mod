using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TouchUp : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TouchUp";

    public TouchUp() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new CardsVar("Select", 1));
        WithTip(CardKeyword.Unplayable);
        WithTip(UnderstudyKeywords.Intense);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    private static bool CanApplyTo(CardModel c) => UnplayableModifier.CanApplyTo(c) && IntenseModifier.CanApplyTo(c);

    protected override bool ShouldGlowGoldInternal =>
        PileType.Hand.GetPile(Owner).Cards.Where(c => c != this).Any(CanApplyTo);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-TOUCH_UP.selectionPrompt"), 0, maxSelect),
            c => c != this && CanApplyTo(c),
            this);
        if (selected == null) return;

        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in selected)
        {
            UnplayableModifier.Remove(card);
            IntenseModifier.Apply(card, CombatState!, allCards);
        }
    }
}
