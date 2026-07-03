using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TouchUp : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TouchUp";

    public TouchUp() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new CardsVar("IntenseSelect", 1));
        WithTip(CardKeyword.Unplayable);
        WithTip(UnderstudyKeywords.Intense);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["IntenseSelect"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        var freed = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-TOUCH_UP.freeSelectionPrompt"), 0, 1),
            c => c != this && UnplayableModifier.CanApplyTo(c),
            this);
        if (freed != null)
            foreach (var card in freed)
                UnplayableModifier.Remove(card);

        int intenseSelect = (int)DynamicVars["IntenseSelect"].BaseValue;
        var intensified = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-TOUCH_UP.intenseSelectionPrompt"), 0, intenseSelect),
            c => c != this && IntenseModifier.CanApplyTo(c),
            this);
        if (intensified != null)
        {
            var allCards = player.Piles.SelectMany(p => p.Cards);
            foreach (var card in intensified)
                IntenseModifier.Apply(card, CombatState!, allCards);
        }
    }
}
