using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Showstopper : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Showstopper";

    public Showstopper() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        // Starts Tense 1 — converts from "always replayable" to "one big hit, then needs
        // freeing," so the damage is raised to compensate.
        WithDamage(34);
        WithTip(UnderstudyKeywords.Tense);
    }

    public override bool IsPreTense => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(8m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
    }
}
