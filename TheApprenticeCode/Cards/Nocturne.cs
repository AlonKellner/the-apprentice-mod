using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Nocturne : ApprenticeCard
{
    public const string CardId = "TheApprentice:Nocturne";

    public Nocturne() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Dream>(upgrade: card.IsUpgraded)));
        WithDreamKeywordTips();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await DreamsAndAmbitions.AddDreams(player, CombatState!, 2, IsUpgraded);
    }
}
