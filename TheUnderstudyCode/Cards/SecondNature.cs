using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Simple, high-cost hit — not tagged Tense (no stack growth, stays a flat hit). It never makes ITSELF
// Unplayable; but whenever it IS Unplayable (from Planned, Tense, Shaken, or any other effect) it
// frees + replays itself once at the end of the turn and ends free — a one-shot free replay per
// Unplayable, not a loop. Only its own Unplayable flag is cleared; any Planned/other modifier is left
// intact, so a Planned Second Nature ends Planned-but-playable.
public class SecondNature : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:SecondNature";

    public SecondNature() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(24);
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(6m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        // Deliberately does NOT make itself Unplayable — being played (by hand, Performance, or its
        // own end-of-turn replay) never locks it. It only responds to Unplayable applied by other
        // effects (see BeforeSideTurnEnd), matching the card text.
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> creatures)
    {
        await base.BeforeSideTurnEnd(context, side, creatures);
        if (side == CombatSide.Player
            && Pile?.Type.IsCombatPile() == true
            && this.TryGetModifier<UnplayableModifier>(out _))
        {
            UnplayableModifier.Remove(this);
            // Target null: CardCmd.AutoPlay rolls a fresh random living enemy for an AnyEnemy
            // card whenever its target argument is null (relied on by Remix.cs/Encore.cs).
            await CardCmd.AutoPlay(context, this, null, AutoPlayType.None, false, false);
            // OnPlay no longer re-adds Unplayable, so the card ends the turn free (a normal playable
            // card). Any Planned slot is untouched — a Planned Second Nature ends Planned-but-playable.
        }
    }
}
