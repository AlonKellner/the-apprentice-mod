using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Hidden combo, appears when you hold BOTH Bright Side and Stage Presence. While present, their separate
// start-of-turn Invert and Swap are replaced by ONE simultaneous Best of Both (Swap N & Invert M at once,
// N = Stage Presence stacks, M = Bright Side stacks) — the same simultaneous engine as Best of Both /
// Trading Fours / Standing Ovation. No card grants it.
//
// This power is a VISIBLE marker + tooltip only: the actual combined effect lives on StagePresencePower
// (with BrightSidePower deferring), keyed on the two powers' real amounts — so correctness never depends
// on this marker's sync timing. Sync just keeps the icon present exactly while both powers are held.
public class LivingTheDreamPower : UnderstudyPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // The pairing condition (pure, unit-testable). Amounts come from the two source powers.
    public static bool ShouldBePresent(int brightSide, int stagePresence) => brightSide > 0 && stagePresence > 0;

    // Idempotent recompute: apply the marker if both powers are present and it's absent, remove it if the
    // pairing is broken and it's present. Re-entrancy safe — applying the marker re-broadcasts
    // AfterPowerAmountChanged, but `have` is then true so it does not re-apply. Uses the generic Apply<T>
    // (clones internally, so no MutableClone needed).
    public static async Task Sync(PlayerChoiceContext ctx, Creature creature)
    {
        bool want = ShouldBePresent(
            creature.GetPowerAmount<BrightSidePower>(), creature.GetPowerAmount<StagePresencePower>());
        var existing = creature.GetPower<LivingTheDreamPower>();
        if (want && existing == null)
            await PowerCmd.Apply<LivingTheDreamPower>(ctx, creature, 1, creature, null, false);
        else if (!want && existing != null)
            await PowerCmd.Remove(existing);
    }
}
