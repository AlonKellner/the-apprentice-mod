using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Regain your composure: block, and clear every jammed card in hand. Remove-Unplayable is
// deliberately uncapped (whole hand).
public class Composure : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Composure";

    public Composure() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(8);
        WithTip(CardKeyword.Unplayable);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override bool ShouldGlowGoldInternal =>
        CardExtensions.AnyUnplayable(PileType.Hand.GetPile(Owner).Cards.Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        var handCards = PileType.Hand.GetPile(cardPlay.Card.Owner).Cards.Where(c => c != this).ToList();
        foreach (var card in handCards.Where(UnplayableModifier.CanApplyTo).ToList())
            UnplayableModifier.Remove(card);
    }
}
