using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// The Tension→Swap combo in one card: load yourself with Tension, then Swap flings it onto every
// enemy (they take it as end-of-turn damage) while you steal their buffs.
public class MethodActing : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:MethodActing";

    public MethodActing() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new SelfDebuffVar("Tension", 3), new IntVar("Swap", 3));
        WithTip(typeof(TensionPower));
        WithTip(UnderstudyKeywords.Swap);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await EmotionalExpression.ApplyTensionToSelf(context, creature, (int)DynamicVars["Tension"].BaseValue, this);
        await SceneStealing.SwapEach(context, creature, (int)DynamicVars["Swap"].BaseValue);
    }
}
