using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Character;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// Shared plumbing for The Understudy's two starting relics (Constant Struggle / Constant Growth):
// both guarantee N chosen cards from the draw pile land in the opening hand (inserted at the Top
// of the Hand pile, so they're genuinely first) and both must subtract that same N from the
// natural turn-1 hand draw so total starting hand size is unaffected. Only SelectCount, the
// selection-prompt loc key, and the turn-1 buff/debuff granted differ per relic.
[Pool(typeof(TheUnderstudyRelicPool))]
public abstract class UnderstudyStarterRelic : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected abstract int SelectCount { get; }
    protected abstract LocString SelectionPrompt { get; }

    private int _grantedThisSetup;

    // Pure: how much to reduce the natural turn-1 hand draw by, given how many cards this relic
    // already placed directly into hand this setup. No engine dependency, directly unit-testable
    // (mirrors FinalLessonPower.ComputeCountdown's precedent for extractable hook math).
    public static decimal ComputeModifiedHandDraw(decimal naturalCount, int grantedThisSetup) =>
        Math.Max(0m, naturalCount - grantedThisSetup);

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        _grantedThisSetup = 0;
        if (player != Owner || Owner?.PlayerCombatState?.TurnNumber != 1) return;

        Flash();

        var pile = PileType.Draw.GetPile(player);
        var selected = await CardSelectCmd.FromCombatPile(choiceContext, pile, player,
            new CardSelectorPrefs(SelectionPrompt, SelectCount, SelectCount));

        foreach (var card in selected)
        {
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Top);
            _grantedThisSetup++;
        }

        await ApplyOnTurnOneSetup(choiceContext, player.Creature);
    }

    protected abstract Task ApplyOnTurnOneSetup(PlayerChoiceContext ctx, Creature creature);

    public override decimal ModifyHandDraw(Player player, decimal count) =>
        player == Owner ? ComputeModifiedHandDraw(count, _grantedThisSetup) : count;
}
