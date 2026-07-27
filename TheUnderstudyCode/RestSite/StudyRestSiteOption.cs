using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using TheUnderstudy.TheUnderstudyCode.Extensions;
using TheUnderstudy.TheUnderstudyCode.Relics;

namespace TheUnderstudy.TheUnderstudyCode.RestSite;

// The "Study" rest-site action, offered by the Book of Endings. Spends the rest action to reveal one
// alternative boss on the current act's map. Mirrors ScoreRestSiteOption in shape; loc in
// rest_site_ui.json under OPTION_THEUNDERSTUDY_STUDY.
public class StudyRestSiteOption : CustomRestSiteOption
{
    public const string Id = "THEUNDERSTUDY_STUDY";
    public override string OptionId => Id;

    // Without this, the button's icon path resolves against the BASE GAME's ui/rest_site/ folder
    // (via ImageHelper.GetImagePath) where option_theunderstudy_study.png can't exist, so the game
    // falls back to a shovel that reads as "Dig". CustomIconPath points at a mod resource instead.
    // PLACEHOLDER: the Planned/order glyph until a dedicated book icon is drawn — swap this for
    // "restsite/study.png".ImagePath() (or similar) once the art exists. Score/Foldable Stage have
    // the same missing-icon gap and would benefit from the same treatment.
    public override string? CustomIconPath => "powers/planned_counter_power.png".ImagePath();

    private readonly BookOfEndings _book;

    public StudyRestSiteOption(Player owner, BookOfEndings book) : base(owner) => _book = book;

    // Visible but unusable once this act's alternative bosses are all revealed — the same treatment
    // Smith gets when every card is already upgraded. IsEnabled greys the button while leaving it
    // hoverable, so the player can still read what the book does instead of the option vanishing. Read
    // live, so in co-op it greys out for the other player as soon as the map reaches the act's cap.
    public override bool IsEnabled => _book.CanStudyNow();

    public override async Task<bool> OnSelect()
    {
        await _book.Study();
        return true;
    }
}
