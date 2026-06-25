using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class JustAsPlanned : ApprenticeCard
{
    public const string CardId = "TheApprentice:JustAsPlanned";

    public JustAsPlanned() : base(3, CardType.Skill, CardRarity.Basic, TargetType.AnyEnemy, false)
    {
        WithKeyword(CardKeyword.Retain, ConstructedCardModel.UpgradeType.None);
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        var planned = PlannedModifier.GetSorted(player.Piles.SelectMany(p => p.Cards));

        foreach (var (card, mod) in planned)
        {
            CardModifier.DirectModifiers(card).Remove(mod);
            await CardCmd.AutoPlay(context, card, cardPlay.Target, AutoPlayType.None, false, false);
        }
    }
}
