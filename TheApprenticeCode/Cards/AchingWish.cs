using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class AchingWish : ApprenticeCard
{
    public const string CardId = "TheApprentice:AchingWish";

    public AchingWish() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(VulnerablePower));
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Dream>(upgrade: card.IsUpgraded)));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var creature = player.Creature;
        await EmotionalExpression.ApplyVulnerableToSelf(context, creature, 1, cardPlay.Card);
        int count = IsUpgraded ? 4 : 3;
        await DreamsAndAmbitions.AddDreams(player, CombatState!, count);
    }
}
