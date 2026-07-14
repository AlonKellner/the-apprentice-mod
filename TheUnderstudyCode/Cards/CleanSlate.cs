using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class CleanSlate : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:CleanSlate";

    public CleanSlate() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(4);
        WithTip(CardKeyword.Unplayable);
        WithTip(UnderstudyKeywords.Tuned);
    }

    public override bool IsPreTuned => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal =>
        CardExtensions.AnyUnplayable(PlannedModifier.RelevantCards(Owner).Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var targets = PlannedModifier.RelevantCards(player).Where(c => c != this && c.IsUnplayable()).ToList();

        foreach (var card in targets)
            if (card.Pile?.Type.IsCombatPile() == true)
                await CardCmd.Exhaust(context, card);

        if (targets.Count > 0)
            await CommonActions.CardAttack(cardPlay.Card, cardPlay, targets.Count).Execute(context);
    }
}
