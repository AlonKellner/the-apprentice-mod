using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;

namespace TheApprentice.TheApprenticeCode.Cards;

public class Epiphany : ApprenticeCard
{
    public const string CardId = "TheApprentice:Epiphany";

    public Epiphany() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(1);
        WithKeyword(ApprenticeKeywords.Expend, ConstructedCardModel.UpgradeType.None);
        WithTip(ApprenticeKeywords.Planned);
    }

    public override bool HasExpend => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await CommonActions.Draw(this, context);

        var selected = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-EPIPHANY.selectionPrompt"), 0, 1),
            c => c != cardPlay.Card && PlannedModifier.CanApplyTo(c),
            this);

        var target = selected?.FirstOrDefault();
        if (target != null)
            PlannedModifier.Apply(target, player.Piles.SelectMany(p => p.Cards));
    }
}
