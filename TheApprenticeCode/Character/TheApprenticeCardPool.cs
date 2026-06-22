using BaseLib.Abstracts;
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

    public override bool ShouldReceiveCombatHooks => true;

    public override Task BeforeCombatStart()
    {
        PlannedTracker.ResetSequence();
        return Task.CompletedTask;
    }
}
