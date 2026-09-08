using System.Collections.Generic;
using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using TheUnderstudy.TheUnderstudyCode.Cards;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Relics;
using PerformanceCard = TheUnderstudy.TheUnderstudyCode.Cards.Workshop;

namespace TheUnderstudy.TheUnderstudyCode.Character;

// A note on the Architect dialogue in localization/eng/ancients.json, because its layout looks like
// a mistake and is not:
//
// For THE_ARCHITECT specifically, BaseLib derives each dialogue's VisitIndex from its index number
// (every other ancient gets a spaced 0, 1, 4, 7... pattern), so dialogue N plays on the Nth meeting.
// The trailing "r" in a key like "1-0r.ancient" sets IsRepeating, which adds that dialogue to the
// pool eligible to be picked at random once the numbered ones run out. The suffix must be on every
// line of a dialogue or on none of them — AncientDialogue.PopulateLines throws on a mix.
//
// The first meeting must NOT repeat: the Architect cannot greet him with "I killed you once already"
// a second time. That needs a dialogue with no "r" — but the STS001 analyzer hardcodes
// "THE_ARCHITECT.talk.SYMBOLID.0-0r.char", "0-0r.next", "0-1r.ancient" and "0-attack" as required
// keys, so slot 0 must exist in the repeating form. Slot 0 also cannot simply be dropped: BaseLib
// finds dialogues with a loop that stops at the first missing index, so a gap at 0 would hide every
// later dialogue.
//
// Hence the layout: slot 0 is a real repeating exchange shaped to those required keys — which is why
// it is the only one that opens on the Understudy's line, and why it is two lines rather than three.
// The first meeting lives in slot 4 with no "r" and "4-visit": "0".
//
// Every repeating slot carries "-visit": "1" so they form one random pool from the second meeting
// onward, rather than each being pinned to its own visit number — a run rarely reaches the Architect
// often enough for the higher-numbered ones to ever be seen otherwise.
//
// Net effect: visit 0 plays slot 4 and never recurs; visit 1 onward picks at random among slots 0-3.
// Renumbering the slots, deleting slot 0, or dropping a "-visit" key will break part of this.
public class TheUnderstudy : PlaceholderCharacterModel
{
    public const string CharacterId = "TheUnderstudy";

    public static readonly Color Color = new("ffffff");

    // The Understudy's gold accent (energy burst / map path). Used anywhere a saturated character color is
    // wanted — including the compendium tile shadow (the relic/potion pools' LabOutlineColor) — since the
    // white NameColor above reads as invisible there.
    public static readonly Color GoldColor = new("f0c040");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    // Map path color: a warm golden tone matching the Understudy's gold palette (the energy-burst
    // f0c040), deliberately brighter and more yellow than the Regent's muted brown-orange (935206)
    // so the two characters' map trails read as clearly distinct.
    public override Color MapDrawingColor => GoldColor;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<UnderstudyStrike>(),
        ModelDb.Card<UnderstudyStrike>(),
        ModelDb.Card<UnderstudyStrike>(),
        ModelDb.Card<UnderstudyStrike>(),
        ModelDb.Card<UnderstudyDefend>(),
        ModelDb.Card<UnderstudyDefend>(),
        ModelDb.Card<UnderstudyDefend>(),
        ModelDb.Card<UnderstudyDefend>(),
        ModelDb.Card<PerformanceCard>(),
        ModelDb.Card<HighNote>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<FalseMask>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<TheUnderstudyCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TheUnderstudyRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TheUnderstudyPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    // Bespoke face art: the in-game top-panel corner portrait (CustomIconTexturePath -> IconTexture)
    // and the character-select face (locked + unlocked) all use Understudy_Face.png. CustomIcon above
    // wraps CustomIconTexturePath, so it picks this up too. The character-select background is a small
    // scene wrapping the Attack card placeholder art as a full-rect TextureRect — CharacterSelectBg is
    // instantiated as a Control, so it must be a scene, not a plain texture.
    public override string CustomIconTexturePath => "character_icon_the_understudy.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_the_understudy.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_the_understudy_locked.png".CharacterUiPath();
    public override string CustomCharacterSelectBg => "char_select_bg_the_understudy.tscn".SceneResPath();

    // ── Drop-in slots: mod art if it is packed, BaseLib's Ironclad placeholder otherwise ──────────
    // PlaceholderCharacterModel fills every one of these with an "ironclad"-keyed base-game path, which
    // is what the Understudy renders today — an Ironclad body in combat, at the rest site, in the shop,
    // on the map, and in a multiplayer lobby. Each override below resolves the mod's own file first and
    // falls back to that inherited default, so shipping the art is the only step needed to switch a slot
    // over; until then these are exactly equivalent to not overriding at all. Naming and dimensions for
    // every file named here are specified in art/assets/CHARACTER_UI.md and art/assets/RIG.md.
    public override string? CustomIconOutlineTexturePath =>
        "character_icon_the_understudy_outline.png".CharacterUiImagePath() ?? base.CustomIconOutlineTexturePath;

    public override string? CustomMapMarkerPath =>
        "map_marker_the_understudy.png".CharacterUiImagePath() ?? base.CustomMapMarkerPath;

    // The combat body. A Tier-1 static illustration and a Tier-3 rigged .glb both arrive as this one
    // scene path, so the rig can replace the static version without touching any other code.
    public override string CustomVisualPath =>
        "the_understudy_visuals.tscn".ExistingSceneResPath() ?? base.CustomVisualPath;

    public override string CustomTrailPath =>
        "the_understudy_card_trail.tscn".ExistingSceneResPath() ?? base.CustomTrailPath;

    public override string CustomRestSiteAnimPath =>
        "the_understudy_rest_site.tscn".ExistingSceneResPath() ?? base.CustomRestSiteAnimPath;

    public override string CustomMerchantAnimPath =>
        "the_understudy_merchant.tscn".ExistingSceneResPath() ?? base.CustomMerchantAnimPath;

    // The character-select -> run wipe. This one is a ShaderMaterial (.tres), not a texture: the shader
    // reads a grayscale mask's red channel and steps it against a threshold, so the art is the mask the
    // material points at. See art/assets/CHARACTER_UI.md.
    public override string CustomCharacterSelectTransitionPath =>
        "materials/the_understudy_transition_mat.tres".ExistingModResPath() ?? base.CustomCharacterSelectTransitionPath;

    // Multiplayer rock-paper-scissors arm art. Live surface: the mod ships multiplayer cards.
    public override string CustomArmPointingTexturePath =>
        "multiplayer_hand_the_understudy_point.png".CharacterUiImagePath() ?? base.CustomArmPointingTexturePath;

    public override string CustomArmRockTexturePath =>
        "multiplayer_hand_the_understudy_rock.png".CharacterUiImagePath() ?? base.CustomArmRockTexturePath;

    public override string CustomArmPaperTexturePath =>
        "multiplayer_hand_the_understudy_paper.png".CharacterUiImagePath() ?? base.CustomArmPaperTexturePath;

    public override string CustomArmScissorsTexturePath =>
        "multiplayer_hand_the_understudy_scissors.png".CharacterUiImagePath() ?? base.CustomArmScissorsTexturePath;

    // Combat energy counter: the Understudy's golden energy orb (big_energy) is the base orb layer,
    // the other four layers stay transparent, with warm-gold burst particles and a dark-amber number
    // outline. BaseLib assembles the animated NEnergyCounter from these five layers (layers 2-3 spin,
    // so the orb lives on the static layer 1) and wraps creation in a try/catch that falls back to the
    // inherited Ironclad orb if anything fails — so this can't break the combat energy display.
    public override CustomEnergyCounter? CustomEnergyCounter => new(
        static layer => (layer == 1 ? "big_energy.png" : "energy_counter_blank.png").CharacterUiPath(),
        outlineColor: new Color("3a2800"),
        burstColor: new Color("f0c040"));
}
