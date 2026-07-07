using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Prompt : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Prompt";

    public Prompt() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(3);
        WithTip(UnderstudyKeywords.Planned);
    }

    public override bool IsPrePlanned => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Draw(this, context);
    }
}
