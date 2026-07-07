using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class DressRehearsal : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:DressRehearsal";

    public DressRehearsal() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithPower<DressRehearsalPower>(2, 1);
        WithTip(typeof(WeakPower));
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(ShakenPower));
        WithTip(typeof(JadedPower));
        WithTip(typeof(LimitedPower));
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int amount = (int)DynamicVars.Power<DressRehearsalPower>().BaseValue;

        await EmotionalExpression.ApplyWeakToSelf(context, creature, amount, this);
        await EmotionalExpression.ApplyVulnerableToSelf(context, creature, amount, this);
        await EmotionalExpression.ApplyShakenToSelf(context, creature, amount, this);
        await EmotionalExpression.ApplyJadedToSelf(context, creature, amount, this);
        await EmotionalExpression.ApplyLimitedToSelf(context, creature, amount, this);

        await CommonActions.Apply<DressRehearsalPower>(context, creature, this);
    }
}
