using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using TheUnderstudy.TheUnderstudyCode.Relics;

namespace TheUnderstudy.TheUnderstudyCode.RestSite;

// The "Study" rest-site action, offered by the Chaotic Book. Spends the rest action to advance the
// book's Study count (three studies transforms it into the Book of Order). Mirrors ScoreRestSiteOption
// in shape; loc in rest_site_ui.json under OPTION_THEUNDERSTUDY_STUDY.
public class StudyRestSiteOption : CustomRestSiteOption
{
    public const string Id = "THEUNDERSTUDY_STUDY";
    public override string OptionId => Id;

    private readonly ChaoticBook _book;

    public StudyRestSiteOption(Player owner, ChaoticBook book) : base(owner) => _book = book;

    public override async Task<bool> OnSelect()
    {
        await _book.Study();
        return true;
    }
}
