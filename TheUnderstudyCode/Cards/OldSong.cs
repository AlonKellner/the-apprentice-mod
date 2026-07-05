using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class OldSong : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:OldSong";

    public OldSong() : base(3, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithVars(new IntVar("Unlimited", 2));
        WithTip(typeof(UnlimitedPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Unlimited"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int amount = (int)DynamicVars["Unlimited"].BaseValue;
        await PowerCmd.Apply<UnlimitedPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
