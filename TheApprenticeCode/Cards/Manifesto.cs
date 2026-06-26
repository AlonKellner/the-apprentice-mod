using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Manifesto : ApprenticeCard
{
    public const string CardId = "TheApprentice:Manifesto";

    public Manifesto() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(1);
        WithTip(typeof(Ambition));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, 4, IsUpgraded);
        await CommonActions.Draw(cardPlay.Card, context);
    }
}
