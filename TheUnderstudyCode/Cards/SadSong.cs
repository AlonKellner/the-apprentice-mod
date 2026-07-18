using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class SadSong : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:SadSong";

    public SadSong() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new IntVar("Unshaken", 2));
        WithTip(typeof(UnshakenPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Unshaken"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int amount = (int)DynamicVars["Unshaken"].BaseValue;
        await PowerCmd.Apply<UnshakenPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
