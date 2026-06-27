using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Resolve : ApprenticeCard
{
    public const string CardId = "TheApprentice:Resolve";

    public Resolve() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Ambition>(upgrade: card.IsUpgraded)));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, 1);

        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;

        foreach (var card in hand.Cards.Where(c => c is Ambition).ToList())
            if (card.TryGetModifier<SpentModifier>(out var mod))
                CardModifier.DirectModifiers(card).Remove(mod);

        if (IsUpgraded)
            foreach (var card in hand.Cards.Where(c => c is Ambition && !c.IsUpgraded).ToList())
                CardCmd.Upgrade(card, CardPreviewStyle.None);
    }
}
