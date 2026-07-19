using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Upstage the whole cast: AoE hit, then push your debuffs onto them. (Swap = Audience / Interaction.)
public class Upstage : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Upstage";

    public Upstage() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        WithDamage(6);
        WithVars(new IntVar("Swap", 1));
        WithTip(UnderstudyKeywords.Swap);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(1m); // Swap -> Swap twice
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        int repeats = (int)DynamicVars["Swap"].BaseValue;
        await SceneStealing.Swap(context, cardPlay.Card.Owner.Creature, repeats);
    }
}
