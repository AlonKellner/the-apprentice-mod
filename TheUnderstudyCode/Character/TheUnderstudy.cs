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
// "THE_ARCHITECT.talk.SYMBOLID.0-0r.char" and friends as required keys, so slot 0 has to exist in the
// repeating form. So slot 0 keeps those keys to satisfy the analyzer and is then neutralised with
// "0-visit": "99": GetValidDialogues only takes an exact VisitIndex == visit count match, and
// AddRepeatingDialogues only pools a dialogue once visits >= its VisitIndex, so slot 0 is unreachable
// on both paths. The real first meeting lives in slot 5 with no "r" and "5-visit": "0".
//
// Net effect: visit 0 plays slot 5 and never recurs; visits 1-4 play slots 1-4; visit 5 onward picks
// at random among slots 1-4. Renumbering the slots or deleting slot 0 will break one half of this.
public class TheUnderstudy : PlaceholderCharacterModel
{
    public const string CharacterId = "TheUnderstudy";

    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

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
        ModelDb.Card<Practice>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<ShamefulGift>()
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
    // instantiated as a Control, so it must be a scene, not a plain texture. The map marker is left as
    // the inherited Ironclad default until dedicated art is made.
    public override string CustomIconTexturePath => "character_icon_the_understudy.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_the_understudy.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_the_understudy_locked.png".CharacterUiPath();
    public override string CustomCharacterSelectBg => "char_select_bg_the_understudy.tscn".SceneResPath();

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
