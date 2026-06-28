using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Commands;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Character;

namespace TheApprentice.TheApprenticeCode.Relics;

[Pool(typeof(TheApprenticeRelicPool))]
public class ConstantStruggle : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<PoeticStruggle>();

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != base.Owner || base.Owner?.PlayerCombatState?.TurnNumber != 1)
            return count;
        return count - 1;
    }

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != base.Owner || base.Owner?.PlayerCombatState?.TurnNumber != 1)
            return;

        Flash();

        var drawPile = player.Piles.FirstOrDefault(p => p.Type == PileType.Draw);
        if (drawPile?.Cards.Count > 0)
        {
            var selected = await MultiPileCardSelect.Select(
                choiceContext, player,
                new CardSelectorPrefs(
                    new LocString("relics", "THEAPPRENTICE-CONSTANT_STRUGGLE.selectionPrompt"), 1, 1),
                _ => true,
                PileType.Draw);

            if (selected != null)
                foreach (var card in selected)
                    await CardPileCmd.Add(card, PileType.Hand);
        }

        await EmotionalExpression.ApplyWeakToSelf(choiceContext, player.Creature, 1, null);
        await EmotionalExpression.ApplyVulnerableToSelf(choiceContext, player.Creature, 1, null);
    }
}
