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

// Simple, high-cost hit — not tagged Tense (no stack growth, stays a flat hit). It self-manages
// UnplayableModifier: OnPlay makes it Unplayable, and whenever it's Unplayable (from its own play,
// Planned, Tense, or anything else) it frees + replays itself ONCE at the end of the turn, then
// leaves itself free (a normal playable card). So each thing that makes it Unplayable grants exactly
// one free end-of-turn replay — it does NOT loop forever. Any Planned/other modifier is left intact;
// only its own Unplayable flag is cleared.
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
        if (!this.TryGetModifier<UnplayableModifier>(out _))
            CardModifier.AddModifier<UnplayableModifier>(this);
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
            // OnPlay re-added Unplayable during that play — strip it again so this card ends the turn
            // free (a normal playable card), rather than looping every turn. Only clears our own flag;
            // any Planned slot is untouched, so a Planned Second Nature stays Planned but playable.
            UnplayableModifier.Remove(this);
        }
    }
}
