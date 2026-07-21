using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// "Best of Both": Swap and Invert resolved SIMULTANEOUSLY. For each of your debuffs, both effects read the
// same starting amount — Invert flips up to {Invert} of it into a buff on you, and Swap pushes it onto every
// enemy — then the debuff is stripped once. So from 1 Weak you become Unweak AND every enemy gains Weak,
// instead of Swap consuming the Weak before Invert could flip it. Swap's TAKE half also runs, stealing each
// enemy's buff — so you keep the good side of your debuffs and grab theirs too. (Swap = Audience, Invert = Self.)
public class GiveAndTake : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:GiveAndTake";

    public GiveAndTake() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new IntVar("Swap", 1), new IntVar("Invert", 1));
        WithTip(UnderstudyKeywords.Swap);
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(1m); // Swap -> Swap twice
        DynamicVars["Invert"].UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var self = cardPlay.Card.Owner.Creature;
        var enemies = self.CombatState!.HittableEnemies.ToList();
        int repeats = (int)DynamicVars["Swap"].BaseValue;
        int swapCap = SceneStealing.SwapCap * repeats;
        int invertMax = (int)DynamicVars["Invert"].BaseValue;

        // Each pair resolves Swap's give + Invert's flip on the same snapshot of its debuff (see
        // InvertiblePair.ResolveGiveAndTake), so the two effects never consume each other's input.
        foreach (var pair in InvertiblePairs.All)
            await pair.ResolveGiveAndTake(context, self, enemies, swapCap, invertMax);

        // Swap's TAKE half: also steal each enemy's swappable buff.
        await SceneStealing.TakeFromEnemies(context, self, repeats);
    }
}
