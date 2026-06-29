using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Manifesto : ApprenticeCard
{
    public const string CardId = "TheApprentice:Manifesto";

    public Manifesto() : base(2, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithCards(1);
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Ambition>(upgrade: card.IsUpgraded)));
        WithAmbitionKeywordTips();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, 4, IsUpgraded);
        await CommonActions.Draw(this, context);
    }
}
