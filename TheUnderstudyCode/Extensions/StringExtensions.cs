using Godot;

namespace TheUnderstudy.TheUnderstudyCode.Extensions;

// Every art path in the mod resolves through here, and every resolver follows one rule: return null
// when the mod ships no art for that slot, so the caller falls through to whatever the base game (or
// BaseLib's Ironclad placeholder) already shows. That is what makes art drop-in — a file appearing in
// the right folder under the right name switches the slot over with no code change, and its absence
// costs nothing. The one exception is PowerImagePath, which deliberately resolves to the game's own
// red "missing" glyph so an art-less power is visibly unfinished rather than silently blank.
//
// See ART.md for the naming contract (slug = Id.Entry minus the THEUNDERSTUDY- prefix, lowercased).
public static class StringExtensions
{
    public static string ImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", path);
    }

    // Resolves under images/<folder>/ and returns null when the file isn't packed.
    private static string? ExistingImage(string folder, string path)
    {
        path = Path.Join(MainFile.ResPath, "images", folder, path);
        return ResourceLoader.Exists(path) ? path : null;
    }

    // The base game's own default for a power icon that has no art yet — a red "NOPE" glyph
    // (res://images/powers/missing_power.png). Used instead of a bundled white placeholder so
    // art-less powers visibly fall back to the game's default rather than an invisible blank.
    private const string MissingPowerIcon = "res://images/powers/missing_power.png";

    // Returns null when no mod-specific portrait exists for this card, so callers fall through to
    // the type placeholder and then to the base game's default (blank) portrait.
    public static string? CardImagePath(this string path) => ExistingImage("card_portraits", path);

    public static string? BigCardImagePath(this string path) => ExistingImage("card_portraits/big", path);

    // The T1 "beta" tier: correct subject and dimensions, unfinished rendering. The base game has a
    // dedicated slot for this (CardModel.BetaPortraitPath), which is how a partly-finished card set
    // ships without ever looking broken.
    public static string? BetaCardImagePath(this string path) => ExistingImage("card_portraits/beta", path);

    public static string PowerImagePath(this string path) => ExistingImage("powers", path) ?? MissingPowerIcon;

    public static string BigPowerImagePath(this string path) => ExistingImage("powers/big", path) ?? MissingPowerIcon;

    // Relic art comes in three pieces (icon / outline / big), fed to BaseLib's RelicImageOverridePatch
    // by UnderstudyRelicArt. Null on miss: RelicIconData treats a null member as "don't override", so
    // an unmade piece leaves the base game's default in place rather than pointing at a 404.
    public static string? RelicImagePath(this string path) => ExistingImage("relics", path);

    public static string? RelicOutlineImagePath(this string path) => ExistingImage("relics/outline", path);

    public static string? BigRelicImagePath(this string path) => ExistingImage("relics/big", path);

    public static string? PotionImagePath(this string path) => ExistingImage("potions", path);

    public static string? PotionOutlineImagePath(this string path) => ExistingImage("potions/outline", path);

    public static string? RestSiteImagePath(this string path) => ExistingImage("restsite", path);

    public static string? CharacterUiImagePath(this string path) => ExistingImage("charui", path);

    public static string CharacterUiPath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "charui", path);
    }

    public static string SceneResPath(this string path)
    {
        return Path.Join(MainFile.ResPath, "scenes", path);
    }

    // Any other mod-relative resource (materials, .tres) resolved the same drop-in way.
    public static string? ExistingModResPath(this string relativePath)
    {
        var path = Path.Join(MainFile.ResPath, relativePath);
        return ResourceLoader.Exists(path) ? path : null;
    }

    // Scene-valued character slots (combat visuals, rest site, merchant) resolve the same way as the
    // image slots: null when the scene isn't shipped, so the caller keeps BaseLib's Ironclad default.
    public static string? ExistingSceneResPath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "scenes", path);
        return ResourceLoader.Exists(path) ? path : null;
    }
}
