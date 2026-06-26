using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Sublimation : ApprenticeCard
{
    public const string CardId = "TheApprentice:Sublimation";

    public Sublimation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(typeof(Dream));
        WithTip(typeof(Ambition));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = player.Piles.Where(p => p.Type == PileType.Hand).SelectMany(p => p.Cards);

        var dreams = hand.Where(c => c is Dream).ToList();

        foreach (var d in dreams)
            await CardCmd.Exhaust(context, d, false, false);
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, dreams.Count, IsUpgraded);
    }
}
