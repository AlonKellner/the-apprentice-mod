using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Fate : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Fate";

    private const int Strikes = 3;

    // Combat-scoped: number of base strikes each Fate has landed this combat. The finisher deals one
    // hit per counted strike (each of base Damage), so its total damage equals the sum of the base
    // damage this card has dealt this combat. Keyed by the (Stable, so persistent) card instance.
    private static ICombatState? _lastCombat;
    private static readonly Dictionary<CardModel, int> _strikesLanded = new();

    public Fate() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        // Stable: fixed statline, never modified — so its per-strike damage stays put as the finisher
        // scales off of it. Also keeps Tuned/Planned off it (CanApplyTo excludes Stable cards).
        WithKeyword(UnderstudyKeywords.Stable, ConstructedCardModel.UpgradeType.None);
        WithTip(UnderstudyKeywords.Stable);
        WithDamage(1);
    }

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
            _strikesLanded.Clear();
        }

        await CommonActions.CardAttack(cardPlay.Card, cardPlay, Strikes).Execute(context);

        _strikesLanded.TryGetValue(this, out int prior);
        int total = prior + Strikes;
        _strikesLanded[this] = total;

        // Finisher: one hit per strike landed this combat, each of base Damage — so its total equals
        // the sum of the damage this card's strikes have dealt this combat.
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, total).Execute(context);
    }
}
