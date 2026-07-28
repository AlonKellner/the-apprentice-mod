using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// A 0-cost, permanent Tuned carrier. Stable freezes it WITH its pre-Tuned stack and, crucially, exempts
// it from Tuned's "becomes Unplayable when played" lock (see Practice's note), so it stays replayable
// all combat — a cheap body that keeps the Tuned count up while chipping 1 damage / 1 Block, both scaled
// by the current Tuned bonus (Tuned 1 x the number of Tuned cards).
public class Repertoire : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Repertoire";

    public Repertoire() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithKeyword(UnderstudyKeywords.Stable, ConstructedCardModel.UpgradeType.None);
        WithDamage(1);
        WithBlock(1);
        WithTip(UnderstudyKeywords.Tuned);
    }

    // Starts each combat carrying Tuned 1 (prints "Tuned 1"; Tuned counts the card itself and grows as
    // more Tuned cards appear). Applied before the Stable snapshot so it freezes with the stack.
    public override bool IsPreTuned => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars.Block.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        await CommonActions.CardBlock(this, cardPlay);
    }
}
