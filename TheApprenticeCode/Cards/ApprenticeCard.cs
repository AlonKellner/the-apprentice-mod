using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Powers;
using TheApprentice.TheApprenticeCode.Character;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

[Pool(typeof(TheApprenticeCardPool))]
public abstract class ApprenticeCard(
    int cost, CardType type, CardRarity rarity, TargetType target,
    bool showInCardLibrary = true)
    : ConstructedCardModel(cost, type, rarity, target, showInCardLibrary)
{
    public override string PortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    public override string CustomPortraitPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    public virtual bool IsPrePlanned => false;

    public virtual bool HasExpend => false;
    public virtual bool ExpendRemovedOnUpgrade => false;

    public override async Task AfterPlayerTurnStartLate(PlayerChoiceContext context, Player player)
    {
        if (player != Owner) return;
        if (!player.Creature.Powers.Any(p => p is PlannedCounterPower))
            await PowerCmd.Apply<PlannedCounterPower>(context, player.Creature, 1m, player.Creature, null, false);
    }

    protected void WithDreamKeywordTips()
    {
        WithTip(ApprenticeKeywords.Dreamy);
        WithTip(ApprenticeKeywords.Expend);
    }

    protected void WithAmbitionKeywordTips()
    {
        WithTip(ApprenticeKeywords.Ambitous);
        WithTip(ApprenticeKeywords.Expend);
    }

    protected void WithPotentialKeywordTips()
    {
        WithTip(ApprenticeKeywords.Ambitous);
        WithTip(ApprenticeKeywords.Dreamy);
        WithTip(ApprenticeKeywords.Expend);
    }

    protected void WithDreamTips()
    {
        WithTip(typeof(Dream));
        WithDreamKeywordTips();
    }

    protected void WithAmbitionTips()
    {
        WithTip(typeof(Ambition));
        WithAmbitionKeywordTips();
    }

    protected void WithPotentialTips()
    {
        WithTip(typeof(Potential));
        WithPotentialKeywordTips();
    }
}
