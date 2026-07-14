using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class UnderstudyDefend : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:UnderstudyDefend";

    public UnderstudyDefend() : base(1, CardType.Skill, CardRarity.Basic, TargetType.None, false)
    {
        WithBlock(5);
        WithTags(CardTag.Defend);
        WithTunedTip();
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);
    }
}
