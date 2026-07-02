using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Agitato : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Agitato";

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
