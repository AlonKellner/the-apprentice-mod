using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// A 0-cost, permanent Tuned carrier. Stable freezes it WITH its pre-Tuned stack and, crucially, exempts
// it from Tuned's "becomes Unplayable when played" lock (see Practice's note), so it stays replayable
// all combat — a cheap body that keeps the Tuned count up. Its damage and Block have ZERO printed base:
// the whole value comes from the Tuned bonus (Tuned stacks x the number of Tuned cards), so it starts at
// its own Tuned amount (1, or 2 upgraded) when alone and scales up as more Tuned cards appear.
public class Repertoire : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Repertoire";

    public Repertoire() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithKeyword(UnderstudyKeywords.Stable, ConstructedCardModel.UpgradeType.None);
        WithDamage(0); // zero printed base; damage is entirely the Tuned bonus (see class note)
        WithBlock(0);  // zero printed base; Block is entirely the Tuned bonus
        WithVars(new IntVar("Tuned", 1)); // the pre-Tuned stack count; upgrades to 2
        WithTip(UnderstudyKeywords.Tuned);
    }

    // Starts each combat carrying Tuned (prints "Tuned N"; Tuned counts the card itself and grows as
    // more Tuned cards appear). Applied before the Stable snapshot so it freezes with the stack. The
    // stack count is the "Tuned" var, so it and the printed number stay in sync — 1, or 2 upgraded
    // (the only pre-Tuned-2 card in the deck). With a zero printed base, the previewed damage/Block is
    // exactly this Tuned amount when the card is alone: 1 base, 2 upgraded.
    public override bool IsPreTuned => true;
    public override int PreTunedStacks => (int)DynamicVars["Tuned"].BaseValue;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Tuned"].UpgradeValueBy(1m); // only Tuned upgrades; the base damage/Block stay 0
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        await CommonActions.CardBlock(this, cardPlay);
    }
}
