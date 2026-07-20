using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Enchantments;

namespace TheUnderstudy.TheUnderstudyCode.RestSite;

// The rest-site action granted by the Score relic: pick a deck card and enchant it with PrePlanned,
// so it starts each combat Planned (in deck order). Mirrors SmithRestSiteOption's select-then-apply
// shape (FromDeckForEnchantment previews the enchant; CardCmd.Enchant applies it to the master-deck
// card, which persists across combats and saves). Loc in rest_site_ui.json (OPTION_<Id>.*).
public class ScoreRestSiteOption : CustomRestSiteOption
{
    public const string Id = "THEUNDERSTUDY_SCORE";
    public override string OptionId => Id;

    public ScoreRestSiteOption(Player owner) : base(owner) { }

    public override async Task<bool> OnSelect()
    {
        var prefs = new CardSelectorPrefs(new LocString("rest_site_ui", "OPTION_" + Id + ".selectionPrompt"), 1)
        {
            Cancelable = true,
            RequireManualConfirmation = true,
        };
        var preview = ModelDb.Enchantment<PrePlanned>().ToMutable();
        var selection = await CardSelectCmd.FromDeckForEnchantment(Owner, preview, 1, prefs);
        if (!selection.Any()) return false;
        foreach (var card in selection)
            CardCmd.Enchant<PrePlanned>(card, 1);
        return true;
    }
}
