using Godot;

namespace TheUnderstudy.TheUnderstudyCode.Extensions;

public static class StringExtensions
{
    public static string ImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", path);
    }

    // The base game's own default for a power icon that has no art yet — a red "NOPE" glyph
    // (res://images/powers/missing_power.png). Used instead of a bundled white placeholder so
    // art-less powers visibly fall back to the game's default rather than an invisible blank.
    private const string MissingPowerIcon = "res://images/powers/missing_power.png";

    // Returns null when no mod-specific portrait exists for this card, so callers fall through to
    // the base game's default (blank) portrait rather than a bundled white placeholder. Once
    // per-card-type placeholder art is added, this can fall back to a type-based image instead.
    public static string? CardImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "card_portraits", path);
        return ResourceLoader.Exists(path) ? path : null;
    }

    public static string? BigCardImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "card_portraits", "big", path);
        return ResourceLoader.Exists(path) ? path : null;
    }

    public static string PowerImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "powers", path);
        return ResourceLoader.Exists(path) ? path : MissingPowerIcon;
    }

    public static string BigPowerImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "powers", "big", path);
        return ResourceLoader.Exists(path) ? path : MissingPowerIcon;
    }

    public static string RelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find relic image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "relics", "relic.png");
    }

    public static string BigRelicImagePath(this string path)
    {
        path = Path.Join(MainFile.ResPath, "images", "relics", "big", path);
        if (ResourceLoader.Exists(path)) return path;

        MainFile.Logger.Info("Could not find big relic image path: " + path);
        return Path.Join(MainFile.ResPath, "images", "relics", "big", "relic.png");
    }

    public static string CharacterUiPath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "charui", path);
    }

    public static string SceneResPath(this string path)
    {
        return Path.Join(MainFile.ResPath, "scenes", path);
    }
}
