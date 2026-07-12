using BaseLib.Abstracts;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using Godot;

namespace TheUnderstudy.TheUnderstudyCode.Character;

public class TheUnderstudyCardPool : CustomCardPoolModel
{
    public override string Title => TheUnderstudy.CharacterId;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // Amber/brass card frame via the base game's HSV recolor shader (res://shaders/hsv.gdshader) —
    // no custom frame art needed. The shader hue-rotates the shared frame texture; H sits just above
    // the base game's orange frame material (h=0.12) toward gold, with V pulled back for a warmer,
    // more metallic brass tone rather than a bright lemon yellow.
    // Tune H in ~0.13–0.20 (lower = more amber/orange, higher = brighter yellow); S = saturation, V = brightness.
    public override float H => 0.14f;
    public override float S => 1.25f;
    public override float V => 1.1f;

    public override Color DeckEntryCardColor => new("c9992e");

    public override bool IsColorless => false;
}
