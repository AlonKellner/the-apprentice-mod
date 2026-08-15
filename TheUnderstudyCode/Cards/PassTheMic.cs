using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Multiplayer Best of Both cast onto a teammate: run the whole Best of Both resolution FOR them (their
// debuffs simultaneously flip into buffs on them and land on the enemies, and each enemy's buff is stolen
// onto them). Shares BestOfBoth.ResolveFor and uses the SAME Swap/Invert values as Best of Both (1/1,
// upgrading to 2/2) — it is simply Best of Both aimed at an ally. (Swap = Audience / Interaction, Invert =
// Self / Positive / Fun.) Named for handing the crowd over to your partner — the room's attention becomes
// theirs for a moment. Was the original Duet.
public class PassTheMic : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:PassTheMic";

    // Only obtainable/playable in co-op — it targets another player. Mirrors the base game's Intercept.
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    // AnyAlly, not AnyPlayer: "another player" excludes yourself (Best of Both already covers your own creature).
    public PassTheMic() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
    {
        WithVars(new IntVar("Swap", 1), new IntVar("Invert", 1));
        WithTip(UnderstudyKeywords.Swap);
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(1m);   // Swap -> Swap twice
        DynamicVars["Invert"].UpgradeValueBy(1m); // 1 -> 2
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // The chosen teammate's creature — the whole Best of Both resolution runs relative to them, exactly
        // as it does for Best of Both's own owner (shared code, so the two never drift). ResolveTarget uses
        // the reticle target on a manual play, prompts here when an ordered resolver auto-plays this, or
        // takes a random ally under Remix; null only when there is no living ally.
        if (await AllyTargeting.ResolveTarget(context, cardPlay) is not { } target) return;
        await BestOfBoth.ResolveFor(context, target,
            (int)DynamicVars["Swap"].BaseValue, (int)DynamicVars["Invert"].BaseValue);
    }
}
