using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// The deck's single pure-Swap card: nothing but the debuff/buff-to-enemy swap.
public class RoleReversal : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:RoleReversal";

    public RoleReversal() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVars(new IntVar("Swap", 2));
        WithTip(UnderstudyKeywords.Swap);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(1m); // Swap twice -> Swap 3 times
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int repeats = (int)DynamicVars["Swap"].BaseValue;
        await SceneStealing.Swap(context, cardPlay.Card.Owner.Creature, repeats);
    }
}
