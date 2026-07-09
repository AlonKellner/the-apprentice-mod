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

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Rehearse : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Rehearse";

    public Rehearse() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(2);
        WithVars(new CardsVar("Select", 2));
        WithTip(UnderstudyKeywords.Tense);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Draw(this, context);

        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-REHEARSE.selectionPrompt"), 0, maxSelect),
            c => c != this && TenseModifier.CanApplyTo(c),
            this);

        if (selected == null) return;
        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in selected)
            TenseModifier.Apply(card, CombatState!, allCards);
    }
}
