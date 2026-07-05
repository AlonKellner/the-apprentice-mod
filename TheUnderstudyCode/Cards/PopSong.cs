using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class PopSong : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:PopSong";

    public PopSong() : base(3, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithVars(new IntVar("Unjaded", 2));
        WithTip(typeof(UnjadedPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Unjaded"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int amount = (int)DynamicVars["Unjaded"].BaseValue;
        await PowerCmd.Apply<UnjadedPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
