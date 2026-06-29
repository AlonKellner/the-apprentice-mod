using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Reverie : ApprenticeCard
{
    public const string CardId = "TheApprentice:Reverie";

    public Reverie() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(1);
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Dream>(upgrade: card.IsUpgraded)));
        WithDreamKeywordTips();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await DreamsAndAmbitions.AddDreams(player, CombatState!, 3, IsUpgraded);
        await CommonActions.Draw(this, context);
    }
}
