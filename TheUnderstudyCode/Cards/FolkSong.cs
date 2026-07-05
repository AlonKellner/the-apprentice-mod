using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class FolkSong : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:FolkSong";

    public FolkSong() : base(3, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithVars(new IntVar("Unweak", 2));
        WithTip(typeof(UnweakPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Unweak"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int amount = (int)DynamicVars["Unweak"].BaseValue;
        await PowerCmd.Apply<UnweakPower>(context, creature, amount, creature, cardPlay.Card, false);
    }
}
