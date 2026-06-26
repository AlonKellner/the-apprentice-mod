using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class ClearMind : ApprenticeCard
{
    public const string CardId = "TheApprentice:ClearMind";

    public ClearMind() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
        WithDamage(2);
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.None);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var toExhaust = player.Piles
            .SelectMany(p => p.Cards)
            .Where(c => c != cardPlay.Card && c.IsUnplayable())
            .ToList();

        foreach (var card in toExhaust)
            await CardCmd.Exhaust(context, card, false, false);

        if (toExhaust.Count > 0)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay, toExhaust.Count)
                .TargetingAllOpponents(CombatState!)
                .Execute(context);
    }
}
