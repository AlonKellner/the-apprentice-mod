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

public class CramSession : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:CramSession";

    public CramSession() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new CardsVar("IntenseSelect", 1));
        WithTip(UnderstudyKeywords.Planned);
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

        var planned = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-CRAM_SESSION.planSelectionPrompt"), 0, 1),
            c => c != this && PlannedModifier.CanApplyTo(c),
            this);
        if (planned != null)
        {
            var allCards = PlannedModifier.RelevantCards(player).ToList();
            foreach (var card in planned)
                PlannedModifier.Apply(card, allCards);
        }

        int intenseSelect = (int)DynamicVars["IntenseSelect"].BaseValue;
        var intensified = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-CRAM_SESSION.intenseSelectionPrompt"), 0, intenseSelect),
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
