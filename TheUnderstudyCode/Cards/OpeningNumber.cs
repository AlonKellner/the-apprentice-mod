using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class OpeningNumber : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:OpeningNumber";

    public OpeningNumber() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
        WithVars(new EnergyVar(2));
        WithTips(_ => new IHoverTip[] { EnergyHoverTip });
        WithKeyword(CardKeyword.Exhaust, ConstructedCardModel.UpgradeType.Remove);
        WithTip(typeof(ShakenPower));
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, player);
        await EmotionalExpression.ApplyShakenToSelf(context, player.Creature, 1, this);
    }
}
