using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// An Attack (for Vigor/attack synergy and Tuned eligibility) that never actually attacks: it deals no
// damage and has no target. Instead it "uses" all your Vigor, converting the amount you had into Block.
// Being an Attack that uses Vigor ties it to Encore — with Encore active it uses Vigor (for the Block)
// but keeps it, exactly like Encore does for damage-dealing attacks.
public class DeceptiveCadence : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:DeceptiveCadence";

    public DeceptiveCadence() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.None)
    {
        // "Block" is the per-Vigor Block granted; total = Block * Vigor you had (computed in OnPlay).
        WithVars(new IntVar("Block", 3));
        WithTip(typeof(VigorPower));
    }

    // Never targets and never deals damage — not even when Tuned. Hard-locked to None (doesn't read
    // Owner, so it's safe on canonical cards) so no modifier or upgrade can give it a reticle, and OnPlay
    // performs no attack so Tuned's damage bonus has nothing to apply to.
    public override TargetType TargetType => TargetType.None;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Block"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int vigor = creature.GetPowerAmount<VigorPower>();   // the Vigor you had
        int perVigor = (int)DynamicVars["Block"].BaseValue;

        if (vigor > 0)
            await CreatureCmd.GainBlock(creature, perVigor * vigor, ValueProp.Unpowered, null, false);

        // Using Vigor spends it — unless Encore is retaining Vigor used this turn.
        if (!EncorePower.IsActive(creature))
            await PowerCmd.Remove<VigorPower>(creature);
    }
}
