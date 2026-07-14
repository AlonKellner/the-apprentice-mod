using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

// Display-only DynamicVar for a self-applied invertible-debuff amount. BaseValue is the native printed
// number; PreviewValue is what will actually land once Pulled Punch softens it, so a loc reference of
// {Name:inverseDiff()} colors it green when it drops below native (less self-harm is good — the inverse
// of damage/block coloring).
//
// The real reduction happens at apply time in InvertTrackerPower's Received-hook interception; this only
// MIRRORS ApathyPower.Dampen for the preview. It deliberately does NOT run the full received hook:
// that interception is a stateful two-phase protocol (_pending -> AfterModifyingPowerAmountReceived) and
// driving it during a preview would leave that state dirty and trip its own invariant on the next real
// application. All self-debuffs in scope are positive debuff-type, so isSignFlip is always false.
public class SelfDebuffVar : DynamicVar
{
    public SelfDebuffVar(string name, decimal amount)
        : base(name, amount)
    {
    }

    public static decimal ComputePreview(decimal baseValue, int pulledPunch) =>
        pulledPunch > 0 ? ApathyPower.Dampen(baseValue, isSignFlip: false, pulledPunch) : baseValue;

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        // Only show the reduced value in a real in-combat preview. Outside combat (card reward, library,
        // deck view) the card is canonical/ownerless — CardModel.Owner throws there — so fall back to the
        // plain native value, matching how the base DamageVar only runs hooks when runGlobalHooks is set.
        if (!runGlobalHooks || !card.IsMutable)
        {
            PreviewValue = BaseValue;
            return;
        }

        int pulledPunch = card.Owner.Creature.GetPowerAmount<ApathyPower>();
        PreviewValue = ComputePreview(BaseValue, pulledPunch);
    }
}
