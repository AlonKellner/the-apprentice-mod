using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Heckle : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Heckle";

    public Heckle() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithCards(2);
        WithTip(typeof(WeakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int count = IsUpgraded ? 3 : 2;

        var selected = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-HECKLE.selectionPrompt"), 0, count),
            c => c != this && c.Keywords.Contains(CardKeyword.Unplayable)
                && (c.Type == CardType.Attack || c.Type == CardType.Skill),
            this);

        if (selected != null)
            foreach (var card in selected)
                if (card.TryGetModifier<UnplayableModifier>(out var mod))
                    CardModifier.DirectModifiers(card).Remove(mod);

        var creature = player.Creature;
        int amount = creature.GetPowerAmount<WeakPower>() + 1;
        await PowerCmd.Apply<WeakPower>(context, cardPlay.Target!, amount, creature, cardPlay.Card, false);
    }
}
