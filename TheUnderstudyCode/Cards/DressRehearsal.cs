using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class DressRehearsal : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:DressRehearsal";

    public DressRehearsal() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithCards(2);
        WithKeyword(CardKeyword.Exhaust);
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var creature = player.Creature;

        await CommonActions.Draw(this, context);
        await PlayerCmd.GainEnergy(1m, player);

        await EmotionalExpression.ApplyWeakToSelf(context, creature, 1, this);
        await EmotionalExpression.ApplyVulnerableToSelf(context, creature, 1, this);
        await EmotionalExpression.ApplyShakenToSelf(context, creature, 1, this);
        await EmotionalExpression.ApplyJadedToSelf(context, creature, 1, this);
        await EmotionalExpression.ApplyLimitedToSelf(context, creature, 1, this);
    }
}
