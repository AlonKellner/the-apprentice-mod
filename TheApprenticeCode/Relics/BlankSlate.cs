using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using TheApprentice.TheApprenticeCode.Character;

namespace TheApprentice.TheApprenticeCode.Relics;

// TheApprenticeB is a minimal prototype character for validating the Planned/Intense loop in
// isolation. The base game's character-select screen unconditionally reads StartingRelics[0]
// (NCharacterSelectScreen.SelectCharacter), so every character needs at least one starting relic
// even if it has no mechanical effect. This relic is intentionally a no-op.
[Pool(typeof(TheApprenticeBRelicPool))]
public class BlankSlate : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Starter;
}
