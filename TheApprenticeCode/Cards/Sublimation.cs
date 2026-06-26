using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Sublimation : ApprenticeCard
{
    public const string CardId = "TheApprentice:Sublimation";

    public Sublimation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new StringVar("AmbitionType", "[gold]Ambition[/gold]"));
        WithTip(CardKeyword.Unplayable);
        WithTip(new BaseLib.Utils.TooltipSource(card => HoverTipFactory.FromCard<Ambition>(upgrade: card.IsUpgraded)));
    }

    protected override void OnUpgrade()
    {
        ((StringVar)DynamicVars["AmbitionType"]).StringValue = "[gold]Ambition+[/gold]";
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = player.Piles.FirstOrDefault(p => p.Type == PileType.Hand);
        if (hand == null) return;

        var unplayable = hand.Cards.Where(c => c.IsUnplayable()).ToList();

        foreach (var c in unplayable)
            await CardCmd.Exhaust(context, c, false, false);
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, unplayable.Count, IsUpgraded);
    }
}
