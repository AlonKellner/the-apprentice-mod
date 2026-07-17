using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class StartOver : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:StartOver";

    public StartOver() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        // Starts Tuned 1 (the "reset button" flavor). Upgrade both drops the cost by 1 and raises
        // the Swap payoff — the big Rare Swap card that unlocks your hand and flings your debuffs
        // onto the enemy team.
        WithCostUpgradeBy(-1);
        WithVars(new IntVar("Swap", 6));
        WithTip(UnderstudyKeywords.Tuned);
        WithTip(CardKeyword.Unplayable);
        WithTip(UnderstudyKeywords.Swap);
    }

    public override bool IsPreTuned => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Swap"].UpgradeValueBy(4m);
    }

    protected override bool ShouldGlowGoldInternal =>
        UnplayableModifier.AnyIn(Owner.Piles.SelectMany(p => p.Cards).Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        foreach (var card in player.Piles.SelectMany(p => p.Cards).Where(c => c != this && UnplayableModifier.CanApplyTo(c)).ToList())
            UnplayableModifier.Remove(card);

        await SceneStealing.SwapEach(context, player.Creature, (int)DynamicVars["Swap"].BaseValue);
    }
}
