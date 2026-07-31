using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Enchantments;
using TheUnderstudy.TheUnderstudyCode.Extensions;

namespace TheUnderstudy.TheUnderstudyCode.RestSite;

// The rest-site action granted by the Drafting Paper relic: pick a deck card and enchant it with
// PrePlanned, so it starts each combat Planned (in deck order). Mirrors SmithRestSiteOption's
// select-then-apply shape (FromDeckForEnchantment previews the enchant; CardCmd.Enchant applies it to the
// master-deck card, which persists across combats and saves). Loc in rest_site_ui.json (OPTION_<Id>.*).
public class DraftingPaperRestSiteOption : CustomRestSiteOption
{
    public const string Id = "THEUNDERSTUDY_DRAFTING_PAPER";
    public override string OptionId => Id;

    // Without a CustomIconPath the button resolves its icon against the base game's ui/rest_site/
    // folder (where a mod texture can't exist) and falls back to the Dig shovel. PLACEHOLDER: the
    // Planned glyph (this option enchants Planned) until dedicated rest-site art is drawn.
    public override string? CustomIconPath => "powers/planned_counter_power.png".ImagePath();

    public DraftingPaperRestSiteOption(Player owner) : base(owner) { }

    public override async Task<bool> OnSelect()
    {
        var prefs = new CardSelectorPrefs(new LocString("rest_site_ui", "OPTION_" + Id + ".selectionPrompt"), 1)
        {
            Cancelable = true,
            RequireManualConfirmation = true,
        };
        // Canonical, NOT mutable: FromDeckForEnchantment calls enchantment.CanEnchant(...), and a mutable
        // model throws MutableModelException there (base-game enchant options all pass the canonical).
        var preview = ModelDb.Enchantment<PrePlanned>();
        var selection = await CardSelectCmd.FromDeckForEnchantment(Owner, preview, 1, prefs);
        if (!selection.Any()) return false;
        foreach (var card in selection)
            CardCmd.Enchant<PrePlanned>(card, 1);
        return true;
    }
}
