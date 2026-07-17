using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Pure Swap payoff — steal the spotlight (and the enemies' buffs), fling your debuffs at them.
public class Limelight : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Limelight";

    public Limelight() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new IntVar("Swap", 3));
        WithTip(UnderstudyKeywords.Swap);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await SceneStealing.SwapEach(context, cardPlay.Card.Owner.Creature, (int)DynamicVars["Swap"].BaseValue);
    }
}
