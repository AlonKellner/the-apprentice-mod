using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class AllNighter : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:AllNighter";

    public AllNighter() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new IntVar("Jaded", 2));
        WithTip(typeof(JadedPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Jaded"].UpgradeValueBy(-1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await PlayerCmd.GainEnergy(1m, player);
        int jaded = (int)DynamicVars["Jaded"].BaseValue;
        await EmotionalExpression.ApplyJadedToSelf(context, player.Creature, jaded, this);
    }
}
