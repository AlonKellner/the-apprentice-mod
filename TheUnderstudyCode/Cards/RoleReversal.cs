using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// PLACEHOLDER reference card for the Swap mechanic — the upcoming card pass will design the real
// Swap cards and likely replace or rework this. It exists so Swap can be exercised in real combat.
public class RoleReversal : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:RoleReversal";

    public RoleReversal() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new IntVar("Swap", 1));
        WithTip(UnderstudyKeywords.Swap);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        int swapAmount = (int)DynamicVars["Swap"].BaseValue;
        await SceneStealing.SwapEach(context, cardPlay.Card.Owner.Creature, swapAmount);
    }
}
