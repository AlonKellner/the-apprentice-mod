using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TouchUp : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TouchUp";

    public TouchUp() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(1);
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(UnvulnerablePower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int count = IsUpgraded ? 2 : 1;

        var selected = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-TOUCH_UP.selectionPrompt"), 0, count),
            c => c != this && c.Keywords.Contains(CardKeyword.Unplayable)
                && (c.Type == CardType.Attack || c.Type == CardType.Skill),
            this);

        if (selected != null)
            foreach (var card in selected)
                if (card.TryGetModifier<UnplayableModifier>(out var mod))
                    CardModifier.DirectModifiers(card).Remove(mod);

        var creature = player.Creature;
        await EmotionalExpression.ConvertVulnerableToUnvulnerable(context, creature, 1);
    }
}
