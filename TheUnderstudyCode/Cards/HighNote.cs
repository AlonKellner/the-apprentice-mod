using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// The starter's third voice: two small hits, a swell of Vigor for whatever comes next, and one
// debuff flipped. Teaches the two mechanics a new player must meet immediately — Vigor as the
// character's damage lever and Invert as its way out of self-inflicted debuffs.
//
// As the Understudy's transcendable starter, High Note is the Ancient hook: implementing
// ITranscendenceCard makes BaseLib register it into the base ArchaicTooth (Orobas node) map, so taking
// that relic transforms this card in the deck into its Ancient form, Standing Ovation.
public class HighNote : UnderstudyCard, ITranscendenceCard
{
    public const string CardId = "TheUnderstudy:HighNote";

    public CardModel GetTranscendenceTransformedCard() => ModelDb.Card<StandingOvation>();

    private const int Hits = 2;

    public HighNote() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, false)
    {
        WithDamage(3);
        WithVars(new IntVar("Vigor", 4), new IntVar("Invert", 1));
        WithMarkedTip(typeof(VigorPower));
        WithTip(UnderstudyKeywords.Invert);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
        DynamicVars["Vigor"].UpgradeValueBy(2m);
    }

    protected override bool ShouldGlowGoldInternal => EmotionalExpression.HasAnyInvertibleDebuffPresent(Owner.Creature);

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        // Vigor is consumed by the NEXT attack command, so gaining it after this card's own hits is
        // deliberate: High Note sets up the following attack rather than pumping itself.
        await CommonActions.CardAttack(cardPlay.Card, cardPlay, Hits).Execute(context);

        var creature = cardPlay.Card.Owner.Creature;
        int vigor = (int)DynamicVars["Vigor"].BaseValue;
        await PowerCmd.Apply<VigorPower>(context, creature, vigor, creature, this, false);

        int invertAmount = (int)DynamicVars["Invert"].BaseValue;
        await EmotionalExpression.InvertEach(context, creature, invertAmount);
    }
}
