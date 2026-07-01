using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Agitato : ApprenticeCardB
{
    public const string CardId = "TheApprentice:Agitato";

    public Agitato() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(3);
        WithTip(typeof(ShakenPower));
    }

    protected override void OnUpgrade() { base.OnUpgrade(); DynamicVars.Cards.UpgradeValueBy(1m); }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await CommonActions.Draw(this, context);
        await EmotionalExpression.ApplyShakenToSelf(context, creature, 1, cardPlay.Card);
    }
}
