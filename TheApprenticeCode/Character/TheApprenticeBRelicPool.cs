using BaseLib.Abstracts;
using TheApprentice.TheApprenticeCode.Extensions;
using Godot;

namespace TheApprentice.TheApprenticeCode.Character;

public class TheApprenticeBRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => TheApprentice.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
