using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Payoff for Unplayable-heavy hands: convert the jam into fuel. Gains 1 Energy per Unplayable card in
// hand. Mirrors Let Loose's live-count preview (each Unplayable drives one unit) and Forte's energy gain.
public class Pumped : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Pumped";

    public Pumped() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        // Live "(Gain N)" preview: N = number of Unplayable cards in hand (each yields 1 Energy).
        // The EnergyVar renders the "1 Energy per" rate as an orb; CalculatedVar shows the total.
        WithVars(
            new EnergyVar(1),
            new CalculationBaseVar(0m),
            new CalculationExtraVar(1m),
            new CalculatedVar("CalculatedEnergy").WithMultiplier(static (card, _) => UnplayableHandCount(card)));
        WithTip(CardKeyword.Unplayable);
        WithCostUpgradeBy(-1); // upgrade: cost 1 -> 0
    }

    // Unplayable cards in hand (excluding this card) — the Energy gained. Shared by the live preview var
    // and OnPlay so the shown count and the real gain can't drift. Static so the CalculatedVar multiplier
    // delegate captures no instance. Matches Let Loose's helper exactly.
    private static int UnplayableHandCount(CardModel card) =>
        PileType.Hand.GetPile(card.Owner).Cards.Count(c => c != card && c.IsUnplayable());

    protected override bool ShouldGlowGoldInternal =>
        PileType.Hand.GetPile(Owner).Cards.Any(c => c != this && c.IsUnplayable());

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int gain = UnplayableHandCount(cardPlay.Card);
        if (gain > 0) await PlayerCmd.GainEnergy(gain, cardPlay.Card.Owner);
    }
}
