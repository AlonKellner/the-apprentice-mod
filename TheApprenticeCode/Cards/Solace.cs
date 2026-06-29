using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Solace : ApprenticeCard
{
    public const string CardId = "TheApprentice:Solace";

    public Solace() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithBlock(8);
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Ambition>(upgrade: card.IsUpgraded)));
        WithAmbitionKeywordTips();
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
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, 1);
    }
}
