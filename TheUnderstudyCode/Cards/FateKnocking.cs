using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

using TheUnderstudy.TheUnderstudyCode.Cards.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class FateKnocking : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:FateKnocking";

    private const int Strikes = 3;

    // Combat-scoped: the ACTUAL damage each FateKnocking's base strikes have dealt this combat (after
    // Strength/Weak/Vulnerable/block — read from the attack's DamageResults). The finisher deals that
    // running sum. Keyed by the card instance; persists across replays of the same card.
    private static ICombatState? _lastCombat;
    private static readonly Dictionary<CardModel, int> _damageDealt = new();

    public FateKnocking() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        // Base: Stable + pre-Tuned (like Practice) — the per-strike base is 0, and its own Tuned 1 makes
        // each strike land for 1 (scaling with more Tuned cards); Stable keeps it from being turned
        // Unplayable. The upgrade removes Stable so it can then be freely buffed/Tuned. The finisher
        // sums the card's real dealt damage either way, so it's no longer coupled to a fixed statline.
        WithKeyword(UnderstudyKeywords.Stable, ConstructedCardModel.UpgradeType.Remove);
        WithTip(UnderstudyKeywords.Stable);
        WithTip(UnderstudyKeywords.Tuned);
        WithVars(new PreTunedDamageVar(0));
    }

    public override bool IsPreTuned => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var combat = cardPlay.Card.Owner.Creature.CombatState!;
        if (!ReferenceEquals(combat, _lastCombat))
        {
            _lastCombat = combat;
            _damageDealt.Clear();
        }

        // The base strikes — capture the ACTUAL damage they deal (after all modifiers and block).
        var strikes = await CommonActions.CardAttack(cardPlay.Card, cardPlay, Strikes).Execute(context);
        int strikeDamage = strikes.Results.SelectMany(r => r).Sum(dr => dr.TotalDamage);

        _damageDealt.TryGetValue(this, out int prior);
        int total = prior + strikeDamage;
        _damageDealt[this] = total;

        // Finisher: deal that running sum as a single hit. Its own damage is not summed back in, so
        // it doesn't compound play-to-play.
        if (total > 0)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay.Target, (decimal)total).Execute(context);
    }
}
