using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// "Every 5 cards that become Unplayable, apply 2 Weak to a random enemy." — counter relic
// (Nunchaku/InkBottle idiom). UnplayableModifier.Applied is a synchronous event, so we count there
// and defer the actual (async) Weak application to a hook that carries a PlayerChoiceContext, the
// same queue-then-flush bridge BalancedPowerBase uses.
public class CueLight : UnderstudyCounterRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    protected override int Threshold => 5;

    public override List<(string, string)>? Localization => new RelicLoc(
        "Cue Light",
        "Every 5 cards that become [gold]Unplayable[/gold], apply 2 [gold]Weak[/gold] to a random enemy.",
        "Every missed entrance is the enemy's cue to falter.");

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromPower<WeakPower>() };

    private int _pendingFires;

    public override Task BeforeCombatStart()
    {
        UnplayableModifier.Applied -= OnUnplayable;
        UnplayableModifier.Applied += OnUnplayable;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        UnplayableModifier.Applied -= OnUnplayable;
        _pendingFires = 0;
        return base.AfterCombatEnd(room);
    }

    private void OnUnplayable(CardModel card)
    {
        if (card.Owner != Owner) return;
        _pendingFires += Bump();
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay) => Flush(context);

    public override Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player) =>
        player == Owner ? Flush(context) : Task.CompletedTask;

    private async Task Flush(PlayerChoiceContext context)
    {
        while (_pendingFires > 0)
        {
            _pendingFires--;
            await ApplyToRandomEnemy<WeakPower>(context, 2);
        }
    }
}
