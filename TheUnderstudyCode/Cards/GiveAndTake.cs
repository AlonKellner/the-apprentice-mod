using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Swap and Invert together — they never clash: Swap pushes your debuffs onto enemies, Invert flips
// what's left into buffs. (Swap = Audience / Interaction, Invert = Self / Positive / Fun.)
public class GiveAndTake : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:GiveAndTake";

    public GiveAndTake() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new IntVar("Swap", 3), new IntVar("Invert", 1));
        WithTip(UnderstudyKeywords.Swap);
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(3m);
        DynamicVars["Invert"].UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await SceneStealing.SwapEach(context, creature, (int)DynamicVars["Swap"].BaseValue);
        await EmotionalExpression.InvertEach(context, creature, (int)DynamicVars["Invert"].BaseValue);
    }
}
