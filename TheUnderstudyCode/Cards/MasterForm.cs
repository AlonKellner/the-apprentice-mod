using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class MasterForm : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:MasterForm";

    public MasterForm() : base(3, CardType.Power, CardRarity.Rare, TargetType.None)
    {
        WithKeyword(CardKeyword.Retain, ConstructedCardModel.UpgradeType.Add);
        WithPowerNoTip<MasterFormPower>(1);
        // The description mentions [gold]Replay[/gold] (which this grants to other cards) but the card
        // never carries Replay itself, so nothing auto-adds its tooltip. Add the base-game static Replay
        // explainer, the same tip HiddenGem uses for granting Replay.
        WithTips(_ => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.ReplayStatic) });
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<MasterFormPower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
