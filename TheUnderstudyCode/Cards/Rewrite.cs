using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Rewrite : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Rewrite";

    public Rewrite() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(1);
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal =>
        UnplayableModifier.AnyIn(PileType.Hand.GetPile(Owner).Cards.Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Draw(this, context);

        var player = cardPlay.Card.Owner;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-REWRITE.selectionPrompt"), 0, 2),
            c => c != this && UnplayableModifier.CanApplyTo(c),
            this);

        if (selected == null) return;
        foreach (var card in selected)
            UnplayableModifier.Remove(card);
    }
}
