using BaseLib.Abstracts;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using Godot;

namespace TheUnderstudy.TheUnderstudyCode.Character;

public class TheUnderstudyCardPool : CustomCardPoolModel
{
    public override string Title => TheUnderstudy.CharacterId;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    public override float H => 1f;
    public override float S => 1f;
    public override float V => 1f;

    public override Color DeckEntryCardColor => new("ffffff");

    public override bool IsColorless => false;
}
