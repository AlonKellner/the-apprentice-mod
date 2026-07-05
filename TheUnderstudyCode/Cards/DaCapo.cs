using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class DaCapo : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:DaCapo";

    public DaCapo() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(3);
        WithVars(new RepeatVar(3));
        WithTip(UnderstudyKeywords.Intense);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Repeat"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int hitCount = (int)DynamicVars["Repeat"].BaseValue;
        Invariants.Check(hitCount > 0, nameof(DaCapo) + "." + nameof(OnPlay),
            $"Repeat resolved to {hitCount} hits — must be positive before attacking and applying Intense");
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, hitCount).Execute(context);

        var player = cardPlay.Card.Owner;
        var allCards = player.Piles.SelectMany(p => p.Cards);
        IntenseModifier.Apply(this, CombatState!, allCards);
    }
}
