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

public class Rehearsal : ApprenticeCard
{
    public const string CardId = "TheApprentice:Rehearsal";

    public Rehearsal() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithCards(3);
        WithTip(ApprenticeKeywords.Planned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await CommonActions.Draw(cardPlay.Card, context);

        int maxChoose = IsUpgraded ? 3 : 2;
        var selected = await CardSelectCmd.FromHand(
            context, player,
            new CardSelectorPrefs(new LocString("cards", "THEAPPRENTICE-REHEARSAL.selectionPrompt"), 0, maxChoose),
            c => c != cardPlay.Card && !c.TryGetModifier<PlannedModifier>(out _),
            this);

        if (selected == null) return;
        foreach (var card in selected)
            PlannedModifier.Apply(card, player.Piles.SelectMany(p => p.Cards));
    }
}
