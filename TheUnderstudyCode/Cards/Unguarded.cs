using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Unguarded : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Unguarded";

    public Unguarded() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(2);
        WithTip(typeof(VulnerablePower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await EmotionalExpression.ApplyVulnerableToSelf(context, cardPlay.Card.Owner.Creature, 2, this);
        await CommonActions.Draw(this, context);
    }
}
