using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Contemplate : ApprenticeCard
{
    public const string CardId = "TheApprentice:Contemplate";

    public Contemplate() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithBlock(6);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(4m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await CommonActions.CardBlock(cardPlay.Card, cardPlay);

        var selected = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-CONTEMPLATE.selectionPrompt"), 0, 1),
            c => c != cardPlay.Card && !c.TryGetModifier<PlannedModifier>(out _),
            this);

        var target = selected?.FirstOrDefault();
        if (target != null)
            PlannedModifier.Apply(target, player.Piles.SelectMany(p => p.Cards));
    }
}
