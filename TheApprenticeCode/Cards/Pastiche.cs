using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Pastiche : ApprenticeCard
{
    public const string CardId = "TheApprentice:Pastiche";

    public Pastiche() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove);
        WithTip(new TooltipSource(_ => HoverTipFactory.FromCard<Potential>(upgrade: true)));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        var selected = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-PASTICHE.selectionPrompt"), 1, 1),
            c => c != cardPlay.Card,
            this);

        var target = selected?.FirstOrDefault();
        if (target == null) return;

        var result = await CardCmd.TransformTo<Potential>(target, CardPreviewStyle.None);
        if (result.HasValue && result.Value.cardAdded != null)
            CardCmd.Upgrade(result.Value.cardAdded, CardPreviewStyle.None);
    }
}
