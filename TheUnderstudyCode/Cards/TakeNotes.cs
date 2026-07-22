using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Draw, then tune what you drew. The Tuned twin of Orchestration. (Tuned = Preparations theme.)
public class TakeNotes : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:TakeNotes";

    public TakeNotes() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithCards(1);
        WithVars(new CardsVar("Select", 1));
        WithTip(UnderstudyKeywords.Tuned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Cards.UpgradeValueBy(1m);
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Draw(this, context);
        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-TAKE_NOTES.selectionPrompt"), 0, maxSelect),
            c => c != this && TunedModifier.CanApplyTo(c),
            this);
        if (selected == null) return;
        foreach (var card in selected)
            TunedModifier.Apply(card);
    }
}
