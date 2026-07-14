using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Signature : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Signature";

    public Signature() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(10);
        WithCards(2);
        WithTip(UnderstudyKeywords.Tuned);
        WithTip(UnderstudyKeywords.Planned);
    }

    // Starts each combat carrying Tuned 1 (a big one-off hit that then needs freeing) and Planned
    // (played via a Planned resolver, in the order it was queued) — mirrors Showstopper's pre-Tuned
    // and the old Prompt/TableRead pre-Planned.
    public override bool IsPreTuned => true;
    public override bool IsPrePlanned => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        await CommonActions.Draw(this, context);
    }
}
