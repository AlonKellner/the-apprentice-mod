using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// A full run-through of the show: sweep the whole cast, then tune two cards for the next pass.
// The mod's only Common AoE attack, and what separates it from Back of my Hand (single target, but
// tunes the WHOLE hand): wide damage + narrow Tuned, versus narrow damage + wide Tuned.
//
// CommonActions.CardAttack dispatches on TargetType internally, so making this AllEnemies needs no
// change to the attack call itself.
public class RunThrough : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:RunThrough";

    private const int MaxTuned = 2;

    public RunThrough() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        WithDamage(5);
        WithTip(UnderstudyKeywords.Tuned);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);

        var player = cardPlay.Card.Owner;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-RUN_THROUGH.selectionPrompt"), 0, MaxTuned),
            c => c != this && TunedModifier.CanApplyTo(c),
            this);

        if (selected == null) return;
        foreach (var card in selected)
            TunedModifier.Apply(card);
    }
}
