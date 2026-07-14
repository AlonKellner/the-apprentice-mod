using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Memorize : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Memorize";

    public Memorize() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithTip(UnderstudyKeywords.Tuned);
        WithTip(CardKeyword.Unplayable);
    }

    protected override Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();
        foreach (var card in allCards.Where(c => c != this && TunedModifier.CanApplyTo(c) && c.IsUnplayable()).ToList())
            TunedModifier.Apply(card, CombatState!, allCards);
        return Task.CompletedTask;
    }
}
