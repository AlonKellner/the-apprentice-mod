using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class CenterStage : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:CenterStage";

    public CenterStage() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithPower<CenterStagePower>(2, 1);
        // Display-only mirror of the applied amount, colored inverseDiff so Pulled Punch shows the
        // reduced self-debuff. Kept in sync with CenterStagePower's value via OnUpgrade below;
        // the separate CenterStagePower var still drives the (unreduced) Invert line.
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
        // Keep the display mirror in step with WithPower<CenterStagePower>(2, 1)'s +1 upgrade.
        DynamicVars["SelfDebuff"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int amount = (int)DynamicVars.Power<CenterStagePower>().BaseValue;

        await EmotionalExpression.ApplyWeakToSelf(context, creature, amount, this);
        await EmotionalExpression.ApplyVulnerableToSelf(context, creature, amount, this);
        await EmotionalExpression.ApplyShakenToSelf(context, creature, amount, this);
        await EmotionalExpression.ApplyJadedToSelf(context, creature, amount, this);
        await EmotionalExpression.ApplyLimitedToSelf(context, creature, amount, this);

        await CommonActions.Apply<CenterStagePower>(context, creature, this);
    }
}
