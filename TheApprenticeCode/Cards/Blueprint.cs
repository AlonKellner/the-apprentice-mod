using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Blueprint : ApprenticeCard
{
    public const string CardId = "TheApprentice:Blueprint";

    public Blueprint() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.None);
        WithTip(CardKeyword.Unplayable);
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Dream>(upgrade: card.IsUpgraded)));
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Ambition>(upgrade: card.IsUpgraded)));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int unplayableCount = player.Piles.SelectMany(p => p.Cards).Count(c => c.IsUnplayable());
        await DreamsAndAmbitions.AddDreams(player, CombatState!, unplayableCount, IsUpgraded);
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, unplayableCount, IsUpgraded);
    }
}
