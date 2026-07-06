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

public class MagnumOpus : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:MagnumOpus";

    public MagnumOpus() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithVars(new CardsVar("Select", 3));
        WithTip(UnderstudyKeywords.Planned);
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
        var pile = PileType.Draw.GetPile(player);
        var selected = await CardSelectCmd.FromCombatPile(
            context,
            pile,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-MAGNUM_OPUS.selectionPrompt"), 0, maxSelect),
            c => PlannedModifier.CanApplyTo(c));

        var allCards = PlannedModifier.RelevantCards(player).ToList();
        foreach (var card in selected)
            PlannedModifier.Apply(card, allCards);
    }
}
