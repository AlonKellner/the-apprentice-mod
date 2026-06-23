using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
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
}
