using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Spend loudness (Vigor) for defense. (Vigor = Sounds theme.)
public class Muffle : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Muffle";

    public Muffle() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(7);
        WithVars(new IntVar("Vigor", 3));
        WithMarkedTip(typeof(VigorPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<VigorPower>(context, creature, -(int)DynamicVars["Vigor"].BaseValue, creature, this, false);
    }
}
