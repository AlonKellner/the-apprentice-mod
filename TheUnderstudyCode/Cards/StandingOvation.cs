using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class StandingOvation : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StandingOvation";

    public StandingOvation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new CardsVar("Select", 1), new IntVar("Vigor", 3));
        WithTip(UnderstudyKeywords.Intense);
        WithTip(typeof(VigorPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Select"].UpgradeValueBy(1m);
        DynamicVars["Vigor"].UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-STANDING_OVATION.selectionPrompt"), 0, maxSelect),
            c => c != this && IntenseModifier.CanApplyTo(c),
            this);

        if (selected != null)
        {
            var allCards = player.Piles.SelectMany(p => p.Cards);
            foreach (var card in selected)
                IntenseModifier.Apply(card, CombatState!, allCards);
        }

        var creature = player.Creature;
        int vigor = (int)DynamicVars["Vigor"].BaseValue;
        await PowerCmd.Apply<VigorPower>(context, creature, vigor, creature, this, false);
    }
}
