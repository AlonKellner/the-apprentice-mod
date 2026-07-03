using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class TakeYourBow : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TakeYourBow";

    public TakeYourBow() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
    {
        WithDamage(5);
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var removed = PileType.Hand.GetPile(player).Cards
            .Where(c => c != this && UnplayableModifier.CanApplyTo(c))
            .ToList();

        foreach (var card in removed)
            UnplayableModifier.Remove(card);

        if (removed.Count == 0) return;
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, removed.Count).Execute(context);
    }
}
