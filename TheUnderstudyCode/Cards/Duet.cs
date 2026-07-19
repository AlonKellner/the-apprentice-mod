using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Multiplayer "ultimate" Give and Take: step in for a castmate — run their whole Swap + Invert turn
// FOR them (push their debuffs onto the enemies, flip what's left into buffs). Cost 1 like Give and
// Take; multiplayer cards run hot. (Swap = Audience / Interaction, Invert = Self / Positive / Fun.)
public class Duet : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Duet";

    // Only obtainable/playable in co-op — it targets another player. Mirrors the base game's Intercept.
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public Duet() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyPlayer)
    {
        WithVars(new IntVar("Swap", 6), new IntVar("Invert", 2));
        WithTip(UnderstudyKeywords.Swap);
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(4m);   // 6 -> 10
        DynamicVars["Invert"].UpgradeValueBy(1m); // 2 -> 3
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // The chosen teammate's creature — Swap/Invert operate relative to them (their debuffs go to
        // the enemies, their remaining debuffs flip), exactly like Give and Take does for its owner.
        if (cardPlay.Target is not { } target) return;
        await SceneStealing.SwapEach(context, target, (int)DynamicVars["Swap"].BaseValue);
        await EmotionalExpression.InvertEach(context, target, (int)DynamicVars["Invert"].BaseValue);
    }
}
