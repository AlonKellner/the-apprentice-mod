using BaseLib.Abstracts;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using Godot;

namespace TheUnderstudy.TheUnderstudyCode.Character;

public class TheUnderstudyRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => TheUnderstudy.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
