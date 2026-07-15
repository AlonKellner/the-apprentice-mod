using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class BuyTime : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:BuyTime";

    public BuyTime() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(9);
        WithVars(new CardsVar("Select", 1));
        WithTip(CardKeyword.Unplayable);
        WithTip(typeof(LimitedPower));
        WithVar(new SelfDebuffVar("Limited", 1));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-BUY_TIME.selectionPrompt"), 0, maxSelect),
            c => c != this && UnplayableModifier.CanApplyTo(c),
            this);
        if (selected != null)
            foreach (var card in selected)
                UnplayableModifier.Remove(card);

        await EmotionalExpression.ApplyLimitedToSelf(context, cardPlay.Card.Owner.Creature, (int)DynamicVars["Limited"].BaseValue, this);
    }
}
