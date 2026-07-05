using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Encore : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Encore";

    public Encore() : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithCostUpgradeBy(-1);
        WithTip(UnderstudyKeywords.Planned);
    }

    // Only require a target when the queue actually has a card that needs one — an empty plan,
    // or a plan of only AoE/self/no-target cards, should just play with no reticle. Owner throws
    // on a canonical (not-yet-instantiated) card model, so fall back to the constructor-seeded
    // value there (bare-construction tests, card library previews, etc).
    public override TargetType TargetType =>
        IsMutable
            ? (PlannedModifier.QueueNeedsEnemyTarget(PlannedModifier.RelevantCards(Owner)) ? TargetType.AnyEnemy : TargetType.None)
            : base.TargetType;

    protected override bool ShouldGlowGoldInternal => PlannedModifier.AnyIn(PlannedModifier.RelevantCards(Owner));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        var allCardsList = PlannedModifier.RelevantCards(player).ToList();
        var planned = PlannedModifier.GetSorted(allCardsList);
        int expectedPlays = planned.Count;
        int actualPlays = 0;
        Log.Info($"Encore.OnPlay: playing {expectedPlays} Planned slot(s), re-queuing eligible cards afterward");
        foreach (var (card, _, slotSeqIdx) in planned)
        {
            PlannedModifier.RemoveSlot(card, slotSeqIdx, allCardsList);
            await CardCmd.AutoPlay(context, card, cardPlay.Target, AutoPlayType.None, false, false);
            actualPlays++;
            if (PlannedModifier.CanApplyTo(card))
                PlannedModifier.Apply(card, PlannedModifier.RelevantCards(player));
        }
        Invariants.CheckEqual(expectedPlays, actualPlays, nameof(Encore) + "." + nameof(OnPlay),
            "Planned cards auto-played");
        PlannedModifier.InvokeChanged();
    }
}
