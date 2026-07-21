using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// "Best of Both": Swap and Invert resolved SIMULTANEOUSLY. For each of your debuffs, both effects read the
// same starting amount — Invert flips up to {Invert} of it into a buff on you, and Swap pushes it onto every
// enemy — then the debuff is stripped once. So from 1 Weak you become Unweak AND every enemy gains Weak,
// instead of Swap consuming the Weak before Invert could flip it. Swap's TAKE half also runs, stealing each
// enemy's buff — so you keep the good side of your debuffs and grab theirs too. (Swap = Audience, Invert = Self.)
public class BestOfBoth : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:BestOfBoth";

    public BestOfBoth() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new IntVar("Swap", 1), new IntVar("Invert", 1));
        WithTip(UnderstudyKeywords.Swap);
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(1m); // Swap -> Swap twice
        DynamicVars["Invert"].UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay) =>
        await ResolveFor(context, cardPlay.Card.Owner.Creature,
            (int)DynamicVars["Swap"].BaseValue, (int)DynamicVars["Invert"].BaseValue);

    // The full Best of Both resolution for one creature — shared with Duet, which runs the exact same thing
    // for a targeted teammate. Interleaved Swap + Invert on the SAME capture -> remove -> apply pipeline as
    // regular Swap (SceneStealing): capture each debuff's give+invert and each enemy's buff to take from the
    // current state, then remove from both sides, then apply to both sides — so interacting powers (an
    // enemy's Artifact, a Weak/Unweak pair) swap instead of cancelling before they're moved.
    public static async Task ResolveFor(
        PlayerChoiceContext context, Creature creature, int swapRepeats, int invertMax)
    {
        var enemies = creature.CombatState!.HittableEnemies.ToList();
        int swapCap = SceneStealing.SwapCap * swapRepeats;

        var plan = new SceneStealing.SwapPlan();
        foreach (var pair in InvertiblePairs.All)
            pair.CaptureGiveAndInvert(plan, creature, enemies, swapCap, invertMax);
        SceneStealing.CaptureTake(plan, creature, enemies, swapCap);
        await SceneStealing.ExecutePlan(context, creature, plan);
    }
}
