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
        WithVars(new CardsVar("Select", 1));
        WithTip(UnderstudyKeywords.Planned);
        WithTip(UnderstudyKeywords.Intense);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-CRAM_SESSION.selectionPrompt"), 0, maxSelect),
            c => c != this && PlannedModifier.CanApplyTo(c) && IntenseModifier.CanApplyTo(c),
            this);
        if (selected == null) return;

        var plannedAllCards = PlannedModifier.RelevantCards(player).ToList();
        var intenseAllCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in selected)
        {
            PlannedModifier.Apply(card, plannedAllCards);
            IntenseModifier.Apply(card, CombatState!, intenseAllCards);
        }
    }
}
