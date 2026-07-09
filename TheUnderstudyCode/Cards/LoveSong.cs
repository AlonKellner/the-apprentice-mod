using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class LoveSong : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:LoveSong";

    public LoveSong() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new IntVar("Unvulnerable", 2));
        WithTip(typeof(UnvulnerablePower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Unvulnerable"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int amount = (int)DynamicVars["Unvulnerable"].BaseValue;
        await PowerCmd.Apply<UnvulnerablePower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
