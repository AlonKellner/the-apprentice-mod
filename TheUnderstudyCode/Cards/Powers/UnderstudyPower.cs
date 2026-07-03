using BaseLib.Abstracts;
using BaseLib.Extensions;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.Cards.Powers;

// Shared base for every custom Power in this deck, mirroring UnderstudyCard's PortraitPath
// pattern: each power automatically gets an icon path derived from its own Id, falling back to a
// placeholder image (via PowerImagePath()/BigPowerImagePath()'s own ResourceLoader.Exists check)
// when no power-specific art has been made yet — instead of no icon/sprite at all.
public abstract class UnderstudyPower : CustomPowerModel
{
    public override string? CustomPackedIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();

    public override string? CustomBigIconPath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}
