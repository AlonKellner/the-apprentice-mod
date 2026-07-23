using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Character;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// "Whenever you apply Tuned to a card with no Tuned, apply an additional Tuned." — the SneckoSkull
// "amplify a status you apply" idiom. TunedModifier.Applied fires only on a card's FIRST-ever Tune,
// so re-applying here lands the card at Tuned 2 and does not re-fire (no recursion). Subscribe
// idempotently per combat and unsubscribe at combat end, exactly like BalancedPowerBase.
[Pool(typeof(TheUnderstudyRelicPool))]
public class Rosin : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;


    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        new[] { HoverTipFactory.FromKeyword(UnderstudyKeywords.Tuned) };

    public override Task BeforeCombatStart()
    {
        TunedModifier.Applied -= OnFirstTuned;
        TunedModifier.Applied += OnFirstTuned;
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        TunedModifier.Applied -= OnFirstTuned;
        return Task.CompletedTask;
    }

    private void OnFirstTuned(CardModel card)
    {
        if (card.Owner != Owner) return;
        var combat = Owner.Creature.CombatState;
        if (combat == null) return;
        Flash();
        TunedModifier.Apply(card);
    }
}
