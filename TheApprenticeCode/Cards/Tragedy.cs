using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using TheApprentice.TheApprenticeCode.Cards.Powers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Tragedy : ApprenticeCard
{
    public const string CardId = "TheApprentice:Tragedy";

    public Tragedy() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.None);
        WithTip(typeof(VulnerablePower));
        WithTip(typeof(TensionPower));
        WithTip(typeof(VigorPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        int removed = await TensionHelper.RemoveAllTension(context, creature, cardPlay.Card);
        int vulnStacks = IsUpgraded ? 5 : 3;
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<VulnerablePower>(context, enemy, vulnStacks, creature, cardPlay.Card, false);
        if (removed > 0)
            await PowerCmd.Apply<VigorPower>(context, creature, removed, creature, cardPlay.Card, false);
    }
}
