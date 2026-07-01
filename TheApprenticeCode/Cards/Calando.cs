using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Calando : ApprenticeCardB
{
    public const string CardId = "TheApprentice:Calando";

    public Calando() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(1);
        WithTip(typeof(ShakenPower));
        WithTip(typeof(UnshakenPower));
    }

    protected override void OnUpgrade() { base.OnUpgrade(); DynamicVars.Cards.UpgradeValueBy(1m); }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.Draw(this, context);
        await EmotionalExpression.ConvertShakenToUnshaken(context, creature, 1);
    }
}
