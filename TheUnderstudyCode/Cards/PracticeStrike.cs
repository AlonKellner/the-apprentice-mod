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

public class PracticeStrike : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:PracticeStrike";

    public PracticeStrike() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(14);
        WithTip(UnderstudyKeywords.Tuned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var player = cardPlay.Card.Owner;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-PRACTICE_STRIKE.selectionPrompt"), 0, 1),
            c => c != this && TunedModifier.CanApplyTo(c),
            this);

        if (selected == null) return;
        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in selected)
            TunedModifier.Apply(card, CombatState!, allCards);
    }
}
