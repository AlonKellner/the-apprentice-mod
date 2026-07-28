using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// The Ancient form of High Note, obtained by transcending it at the Orobas node. The starter's small
// two-hit / Vigor / Invert swells into a full performance: three hits, a big Vigor surge, and a
// SIMULTANEOUS Swap + Invert. Reuses BestOfBoth.ResolveFor so the Swap and Invert both draw from each
// debuff's same starting stacks (one capture -> remove -> apply pipeline) rather than one consuming the
// debuff before the other can act.
public class StandingOvation : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StandingOvation";

    private const int Hits = 3;

    public StandingOvation() : base(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
        WithDamage(4);
        WithVars(new IntVar("Vigor", 6));
        WithMarkedTip(typeof(VigorPower));
        WithTip(UnderstudyKeywords.Swap);
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(2m);   // 4 -> 6
        DynamicVars["Vigor"].UpgradeValueBy(4m);  // 6 -> 10
        // Swap/Invert are already "ALL" — nothing to scale on upgrade.
    }

    protected override bool ShouldGlowGoldInternal =>
        EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, Hits).Execute(context);

        // Vigor is consumed by the NEXT attack command, so gaining it after this card's own hits sets up
        // the following attack rather than pumping this one (same as High Note).
        var creature = cardPlay.Card.Owner.Creature;
        int vigor = (int)DynamicVars["Vigor"].BaseValue;
        await PowerCmd.Apply<VigorPower>(context, creature, vigor, creature, this, false);

        await BestOfBoth.ResolveAllFor(context, creature);
    }
}
