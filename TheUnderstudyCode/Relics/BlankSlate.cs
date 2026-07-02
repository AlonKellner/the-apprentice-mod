using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheUnderstudy.TheUnderstudyCode.Character;

namespace TheUnderstudy.TheUnderstudyCode.Relics;

// The base game's character-select screen unconditionally reads StartingRelics[0]
// (NCharacterSelectScreen.SelectCharacter), so every character needs at least one starting relic
// even if it has no mechanical effect. This relic is intentionally a no-op.
[Pool(typeof(TheUnderstudyRelicPool))]
public class BlankSlate : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;
}
