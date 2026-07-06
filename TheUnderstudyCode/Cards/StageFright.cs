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

    public StageFright() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(UnderstudyKeywords.Intense);
        WithTip(CardKeyword.Unplayable);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = PileType.Hand.GetPile(player);
        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in hand.Cards.Where(c => c != this && IntenseModifier.CanApplyTo(c) && c.IsUnplayable()).ToList())
            IntenseModifier.Apply(card, CombatState!, allCards);
    }
}
