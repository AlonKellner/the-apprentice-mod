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

// The rest-site action granted by the Foldable Stage relic: pick a deck card and enchant it with
// PreTuned, so it starts each combat with Tuned. Same select-then-apply shape as ScoreRestSiteOption.
public class FoldableStageRestSiteOption : CustomRestSiteOption
{
    public const string Id = "THEUNDERSTUDY_FOLDABLE_STAGE";
    public override string OptionId => Id;

    public FoldableStageRestSiteOption(Player owner) : base(owner) { }

    public override async Task<bool> OnSelect()
    {
        var prefs = new CardSelectorPrefs(new LocString("rest_site_ui", "OPTION_" + Id + ".selectionPrompt"), 1)
        {
            Cancelable = true,
            RequireManualConfirmation = true,
        };
        var preview = ModelDb.Enchantment<PreTuned>().ToMutable();
        var selection = await CardSelectCmd.FromDeckForEnchantment(Owner, preview, 1, prefs);
        if (!selection.Any()) return false;
        foreach (var card in selection)
            CardCmd.Enchant<PreTuned>(card, 1);
        return true;
    }
}
