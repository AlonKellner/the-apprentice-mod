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
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class MissedCue : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:MissedCue";

    public MissedCue() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
        WithVars(new CardsVar("Select", 2));
        WithTip(CardKeyword.Unplayable);
        WithTip(typeof(ShakenPower));
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars["Select"].UpgradeValueBy(1m);
    }

    protected override bool ShouldGlowGoldInternal =>
        UnplayableModifier.AnyIn(PileType.Hand.GetPile(Owner).Cards.Where(c => c != this));

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        int maxSelect = (int)DynamicVars["Select"].BaseValue;
        var freed = await CardSelectCmd.FromHand(
            context,
            player,
            new CardSelectorPrefs(new LocString("cards", "THEUNDERSTUDY-MISSED_CUE.selectionPrompt"), 0, maxSelect),
            c => c != this && UnplayableModifier.CanApplyTo(c),
            this);
        if (freed != null)
            foreach (var card in freed)
                UnplayableModifier.Remove(card);

        await EmotionalExpression.ApplyShakenToSelf(context, player.Creature, 1, this);
    }
}
