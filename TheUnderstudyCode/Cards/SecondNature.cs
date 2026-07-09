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

// Simple, high-cost, plainly repeating hit — not tagged Intense (no stack growth, stays a flat
// hit every cycle): it self-manages UnplayableModifier directly, becoming Unplayable after each
// play and freeing + replaying itself at the end of every turn from then on, for free.
public class SecondNature : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:SecondNature";

    public SecondNature() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(8);
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(4m);
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
        }
    }
}
