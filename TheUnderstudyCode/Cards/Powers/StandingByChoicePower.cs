using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Player-choice variant — applied by the upgraded Standing By+ card. Each trigger prompts the player
// to free up to Amount eligible cards in hand (MinSelect 0, so declining is allowed). Stacks
// independently of StandingByPower (the random variant); both share the "Standing By" badge.
public class StandingByChoicePower : StandingByPowerBase
{
    protected override string SelectionFragment => "attack or skill of your choice";

    protected override async Task<IReadOnlyList<CardModel>> SelectCards(
        PlayerChoiceContext context, Player player, IReadOnlyList<CardModel> candidates, int count)
    {
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-STANDING_BY.selectionPrompt"), 0, count),
            candidates.Contains,
            this);
        return selected?.ToList() ?? new List<CardModel>();
    }
}
