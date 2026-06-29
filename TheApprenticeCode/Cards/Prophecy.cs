using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Prophecy : ApprenticeCard
{
    public const string CardId = "TheApprentice:Prophecy";

    public Prophecy() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithTip(ApprenticeKeywords.Planned);
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Dream>(upgrade: card.IsUpgraded)));
        WithDreamKeywordTips();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var creature = player.Creature;
        int weakStacks = creature.GetPowerAmount<WeakPower>();
        int vulStacks = creature.GetPowerAmount<VulnerablePower>();
        int count = EmotionalExpression.CountUniqueDebuffTypes(weakStacks, vulStacks);
        if (count <= 0) return;

        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        var before = new HashSet<CardModel>(hand?.Cards ?? Enumerable.Empty<CardModel>());

        await DreamsAndAmbitions.AddDreams(player, CombatState!, count, IsUpgraded);

        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();
        foreach (var drawnCard in hand?.Cards.Where(c => !before.Contains(c)).ToList() ?? [])
            PlannedModifier.Apply(drawnCard, allCards);
    }
}
