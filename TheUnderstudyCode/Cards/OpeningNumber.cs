using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class OpeningNumber : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:OpeningNumber";

    public OpeningNumber() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(3);
        WithTip(typeof(ShakenPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Draw(this, context);
        await EmotionalExpression.ApplyShakenToSelf(context, cardPlay.Card.Owner.Creature, 1, this);
    }
}
