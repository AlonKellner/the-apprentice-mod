using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Defensive Swap — gain Block, then trade fortunes with the enemy team.
public class BodyDouble : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:BodyDouble";

    public BodyDouble() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithBlock(8);
        WithVars(new IntVar("Swap", 1));
        WithTip(UnderstudyKeywords.Swap);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        await SceneStealing.Swap(context, cardPlay.Card.Owner.Creature, (int)DynamicVars["Swap"].BaseValue);
    }
}
