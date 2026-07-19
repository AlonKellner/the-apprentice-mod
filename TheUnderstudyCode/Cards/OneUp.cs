using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class OneUp : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:OneUp";

    public OneUp() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(2);
        WithVars(new RepeatVar(3));
        WithTip(UnderstudyKeywords.Tuned);
    }

    // Starts each combat pre-Tuned (Tuned 1), then applies Tuned to itself again on play — so it both
    // counts toward every Tuned card's bonus from turn 1 and keeps ramping as it's replayed.
    public override bool IsPreTuned => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Repeat"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int hitCount = (int)DynamicVars["Repeat"].BaseValue;
        Invariants.Check(hitCount > 0, nameof(OneUp) + "." + nameof(OnPlay),
            $"Repeat resolved to {hitCount} hits — must be positive before attacking and applying Tuned");
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, hitCount).Execute(context);

        var player = cardPlay.Card.Owner;
        var allCards = player.Piles.SelectMany(p => p.Cards);
        // Granted after the attack above, so this play's damage didn't benefit from it — pass
        // cardPlay so a first-ever application doesn't lock this card up for THIS play (see
        // TunedModifier.Apply's grantedAfterOwnCheck doc).
        TunedModifier.Apply(this, CombatState!, allCards, cardPlay);
    }
}
