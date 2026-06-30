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

public class Lucidity : ApprenticeCard
{
    public const string CardId = "TheApprentice:Lucidity";

    public Lucidity() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Dream>(upgrade: card.IsUpgraded)));
        WithDreamKeywordTips();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await DreamsAndAmbitions.AddDreams(player, CombatState!, 1);

        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;

        foreach (var card in hand.Cards.Where(c => c is Dream).ToList())
            if (card.TryGetModifier<ExpendModifier>(out var mod) && mod.IsSpent)
                mod.Reset();

        if (IsUpgraded)
            foreach (var card in hand.Cards.Where(c => c is Dream && !c.IsUpgraded).ToList())
                CardCmd.Upgrade(card, CardPreviewStyle.None);
    }
}
