using TheApprentice.TheApprenticeCode.Cards.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Strings : ApprenticeCard
{
    public const string CardId = "TheApprentice:Strings";

    public Strings() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None)
    {
        WithCards(1);
        WithTip(typeof(TensionPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int draws = IsUpgraded ? 3 : 2;
        for (int i = 0; i < draws; i++)
            await CommonActions.Draw(this, context);
        await TensionHelper.AddTension(context, cardPlay.Card.Owner.Creature, 3, cardPlay.Card);
    }
}
