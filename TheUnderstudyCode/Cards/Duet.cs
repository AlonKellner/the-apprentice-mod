using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Multiplayer "ultimate" Best of Both: step in for a castmate — run the whole Best of Both resolution FOR
// them (their debuffs simultaneously flip into buffs on them and land on the enemies, and each enemy's buff
// is stolen onto them). Shares BestOfBoth.ResolveFor so it stays identical. Cost 1; multiplayer cards run
// hot. (Swap = Audience / Interaction, Invert = Self / Positive / Fun.)
public class Duet : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Duet";

    // Only obtainable/playable in co-op — it targets another player. Mirrors the base game's Intercept.
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public Duet() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyPlayer)
    {
        WithVars(new IntVar("Swap", 2), new IntVar("Invert", 2));
        WithTip(UnderstudyKeywords.Swap);
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(1m);   // Swap twice -> Swap 3 times
        DynamicVars["Invert"].UpgradeValueBy(1m); // 2 -> 3
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // The chosen teammate's creature — the whole Best of Both resolution runs relative to them, exactly
        // as it does for Best of Both's own owner (shared code, so the two never drift).
        if (cardPlay.Target is not { } target) return;
        await BestOfBoth.ResolveFor(context, target,
            (int)DynamicVars["Swap"].BaseValue, (int)DynamicVars["Invert"].BaseValue);
    }
}
