using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Experience : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Experience";

    public Experience() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        // Base 0 on purpose: the card is always pre-Tuned and its damage is purely its Tuned bonus, so the
        // dynamic base damage shown/dealt equals the total Tuned across the deck.
        WithDamage(0);
        WithTip(UnderstudyKeywords.Tuned);
        WithTip(UnderstudyKeywords.Planned);
    }

    public override bool IsPreTuned => true;

    // Upgrade grants "starts each combat Planned" (see loc's IfUpgraded branch), mirroring Playlist.
    public override bool IsPrePlanned => IsUpgraded;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        var player = cardPlay.Card.Owner;
        foreach (var card in PlannedModifier.RelevantCards(player))
            TunedModifier.DoubleStacks(card);
    }
}
