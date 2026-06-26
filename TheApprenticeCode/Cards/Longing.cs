using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Longing : ApprenticeCard
{
    public const string CardId = "TheApprentice:Longing";

    public Longing() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(5);
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Dream>(upgrade: card.IsUpgraded)));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
        await DreamsAndAmbitions.AddDreams(player, CombatState!, 2, IsUpgraded);
    }
}
