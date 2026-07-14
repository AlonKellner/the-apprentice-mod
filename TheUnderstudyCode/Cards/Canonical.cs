using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Canonical : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Canonical";

    public Canonical() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove);
        WithTip(UnderstudyKeywords.Stable);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-CANONICAL.selectionPrompt"), 0, 1),
            c => c != this && StableModifier.CanApplyTo(c),
            this);

        if (selected == null) return;
        foreach (var card in selected)
            StableModifier.Apply(card);
    }
}
