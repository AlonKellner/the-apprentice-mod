using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class DressRehearsal : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:DressRehearsal";

    public DressRehearsal() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithPower<DressRehearsalPower>(2, 1);
        // Display-only mirror of the applied amount, colored inverseDiff so Pulled Punch shows the
        // reduced self-debuff. Kept in sync with DressRehearsalPower's value via OnUpgrade below;
        // the separate DressRehearsalPower var still drives the (unreduced) Invert line.
        WithVar(new SelfDebuffVar("SelfDebuff", 2));
        WithInvertibleTip(typeof(WeakPower));
        WithInvertibleTip(typeof(VulnerablePower));
        WithTip(typeof(ShakenPower));
        WithTip(typeof(JadedPower));
        WithTip(typeof(LimitedPower));
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        // Keep the display mirror in step with WithPower<DressRehearsalPower>(2, 1)'s +1 upgrade.
        DynamicVars["SelfDebuff"].UpgradeValueBy(1m);
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
