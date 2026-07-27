using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Rare "cash out your Tuned" card: every Tuned card becomes Planned (queued for next turn in deck order),
// then Tuned is stripped from every card. Turns a built-up Tuned board into a big Planned turn at the
// cost of the Tuned bonus itself. No selection — it acts on all Tuned cards at once — so it needs no
// badge arming; the modifier changes are plain game-state mutations that run on every co-op client.
public class Spectacle : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Spectacle";

    public Spectacle() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.None);
        WithCostUpgradeBy(-1);
        WithTip(UnderstudyKeywords.Planned);
        WithTip(UnderstudyKeywords.Tuned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        // Capture the Tuned cards first: the description order is Plan-then-reset, and stripping Tuned
        // afterwards mutates the very set we are iterating.
        var tuned = TunedModifier.TunedCards(player).ToList();

        foreach (var card in tuned)
            if (PlannedModifier.CanApplyTo(card))
                PlannedModifier.Apply(card, CombatState!);
        PlannedModifier.InvokeChanged();

        foreach (var card in tuned)
            if (card.TryGetModifier<TunedModifier>(out var mod))
                CardModifier.DirectModifiers(card).Remove(mod!);

        await Task.CompletedTask;
    }
}
