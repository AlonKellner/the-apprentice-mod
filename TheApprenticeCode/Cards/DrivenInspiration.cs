using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class DrivenInspiration : ApprenticeCard
{
    public const string CardId = "TheApprentice:DrivenInspiration";

    public DrivenInspiration() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithTip(ApprenticeKeywords.Planned);
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Dream>(upgrade: card.IsUpgraded)));
        WithTip(new TooltipSource(card => HoverTipFactory.FromCard<Ambition>(upgrade: card.IsUpgraded)));
        WithDreamKeywordTips();
        WithAmbitionKeywordTips();
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await DreamsAndAmbitions.AddDreams(player, CombatState!, 1, IsUpgraded);
        await DreamsAndAmbitions.AddAmbitions(player, CombatState!, 1, IsUpgraded);

        var selected = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-DRIVEN_INSPIRATION.selectionPrompt"), 0, 1),
            c => c != cardPlay.Card && PlannedModifier.CanApplyTo(c),
            this);

        var target = selected?.FirstOrDefault();
        if (target != null)
            PlannedModifier.Apply(target, player.Piles.SelectMany(p => p.Cards));
    }
}
