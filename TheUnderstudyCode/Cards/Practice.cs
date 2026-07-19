using System.Linq;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Modifiers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class Practice : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Practice";

    public Practice() : base(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, false)
    {
        WithKeyword(UnderstudyKeywords.Stable, ConstructedCardModel.UpgradeType.None);
        WithDamage(0);
        WithVars(new CardsVar("Select", 2));
        WithTip(UnderstudyKeywords.Tuned);
    }

    // Stable + pre-Tuned: Practice starts each combat carrying Tuned 1 (so it grows in value as more
    // Tuned cards are created this combat), but Stable keeps it from ever becoming Unplayable.
    public override bool IsPreTuned => true;

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(cardPlay.Card, cardPlay).Execute(context);
        var player = cardPlay.Card.Owner;
        var maxSelect = IsUpgraded ? 3 : 2;
        var selected = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-PRACTICE.selectionPrompt"), 0, maxSelect),
            c => c != this && TunedModifier.CanApplyTo(c),
            this);

        if (selected == null) return;
        var allCards = player.Piles.SelectMany(p => p.Cards);
        foreach (var card in selected)
            TunedModifier.Apply(card, CombatState!, allCards);
    }
}
