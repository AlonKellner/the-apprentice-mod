using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class JustAsPlanned : ApprenticeCard
{
    public const string CardId = "TheApprentice:JustAsPlanned";

    public JustAsPlanned() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithKeyword(CardKeyword.Retain, ConstructedCardModel.UpgradeType.None);
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;

        var planned = new List<(CardModel card, PlannedModifier mod)>();
        foreach (var pile in player.Piles)
        {
            foreach (var card in pile.Cards.ToList())
            {
                if (card.TryGetModifier<PlannedModifier>(out var mod))
                    planned.Add((card, mod));
            }
        }

        planned.Sort((a, b) => a.mod.SequenceIndex.CompareTo(b.mod.SequenceIndex));

        foreach (var (card, mod) in planned)
        {
            CardModifier.DirectModifiers(card).Remove(mod);
            await CardCmd.AutoPlay(context, card, cardPlay.Target, AutoPlayType.None, false, false);
        }
    }
}
