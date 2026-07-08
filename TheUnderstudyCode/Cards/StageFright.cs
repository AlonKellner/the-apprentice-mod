using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class StageFright : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StageFright";

    public StageFright() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCostUpgradeBy(-1);
        WithTip(UnderstudyKeywords.Intense);
        WithTip(CardKeyword.Unplayable);
    }

    protected override Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var allCards = player.Piles.SelectMany(p => p.Cards).ToList();
        foreach (var card in allCards.Where(c => c != this && IntenseModifier.CanApplyTo(c) && c.IsUnplayable()).ToList())
            IntenseModifier.Apply(card, CombatState!, allCards);
        return Task.CompletedTask;
    }
}
