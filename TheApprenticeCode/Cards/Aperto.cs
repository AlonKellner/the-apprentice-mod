using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Aperto : ApprenticeCardB
{
    public const string CardId = "TheApprentice:Aperto";

    public Aperto() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(1);
        WithTip(typeof(LimitedPower));
        WithTip(typeof(UnlimitedPower));
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
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-APERTO.selectionPrompt"), 0, count),
            c => c != this && c.Keywords.Contains(CardKeyword.Unplayable),
            this);

        if (selected != null)
            foreach (var card in selected)
                if (card.TryGetModifier<UnplayableModifier>(out var mod))
                    CardModifier.DirectModifiers(card).Remove(mod);

        var creature = player.Creature;
        if (creature.GetPowerAmount<LimitedPower>() > 0)
        {
            await PowerCmd.Apply<LimitedPower>(context, creature, -1m, creature, cardPlay.Card, false);
            await PowerCmd.Apply<UnlimitedPower>(context, creature, 1m, creature, cardPlay.Card, false);
        }
    }
}
