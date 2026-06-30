using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;
using TheApprentice.TheApprenticeCode.Extensions;
using Godot;

namespace TheApprentice.TheApprenticeCode.Character;

public class TheApprenticeCardPool : CustomCardPoolModel
{
    public override string Title => TheApprentice.CharacterId;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    public override float H => 1f;
    public override float S => 1f;
    public override float V => 1f;

    public override Color DeckEntryCardColor => new("ffffff");

    public override bool IsColorless => false;

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card is ApprenticeCard { IsPrePlanned: true } && !card.TryGetModifier<PlannedModifier>(out _))
        {
            CardModifier.AddModifier<PlannedModifier>(card);
            if (card.TryGetModifier<PlannedModifier>(out var mod))
                mod.SequenceIndex = -1;
        }
        if (card is ApprenticeCard { HasExpend: true } apprenticeCard
            && !(apprenticeCard.ExpendRemovedOnUpgrade && card.IsUpgraded))
        {
            if (!card.TryGetModifier<ExpendModifier>(out var expendMod))
                CardModifier.AddModifier(card, new ExpendModifier());
            else
                expendMod.Reset();
        }
        return Task.CompletedTask;
    }
}
