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

public class Confidence : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Confidence";

    public Confidence() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithBlock(8);
        WithVars(new CardsVar("Select", 1));
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal =>
        UnplayableModifier.AnyIn(PileType.Hand.GetPile(Owner).Cards.Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);

        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-CONFIDENCE.selectionPrompt"), 0, maxSelect),
            c => c != this && UnplayableModifier.CanApplyTo(c),
            this);

        if (selected == null) return;
        foreach (var card in selected)
            UnplayableModifier.Remove(card);
    }
}
