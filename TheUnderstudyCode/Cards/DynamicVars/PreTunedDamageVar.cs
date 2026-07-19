using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

// Damage var for pre-Tuned cards. Their printed base bakes OUT the +1 their own starting Tuned 1
// provides — the base is one lower than the intended hit, and the Tuned stack makes up the
// difference. In combat the live TunedModifier adds Stacks × (# distinct Tuned cards this combat) via
// Hook.ModifyDamage, so the inherited DamageVar preview already shows the full scaled value. Out of
// combat (deck / reward / library) there is no live modifier, so we add the own-Tuned contribution
// here so the card still previews its intended damage (base + own Tuned) rather than the bare base.
public class PreTunedDamageVar : DamageVar
{
    // 8 == the powered-attack ValueProp WithDamage uses, so Tuned/Strength/etc. modify this hit.
    private const ValueProp PoweredAttack = (ValueProp)8;

    private readonly int _ownTunedStacks;

    public PreTunedDamageVar(decimal baseDamage, int ownTunedStacks = 1)
        : base(baseDamage, PoweredAttack)
    {
        _ownTunedStacks = ownTunedStacks;
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);
        // In combat (runGlobalHooks) the live TunedModifier already contributes through Hook.ModifyDamage;
        // only out of combat do we add the own Tuned stack the modifier isn't present to supply.
        if (!runGlobalHooks)
            PreviewValue += _ownTunedStacks;
    }
}
