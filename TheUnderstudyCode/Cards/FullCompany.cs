using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class FullCompany : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:FullCompany";

    public FullCompany() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithTip(UnderstudyKeywords.Intense);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var hand = PileType.Hand.GetPile(player);
        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in hand.Cards.Where(c => c != this && IntenseModifier.CanApplyTo(c)).ToList())
            IntenseModifier.Apply(card, CombatState!, allCards);
    }
}
