using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

// Capstone: cost 4 is intentionally near-uncastable by hand — the upgrade makes it start each combat
// Planned so it plays for free (and even the base card can be "cheated" out via any Planned-applying
// card). Grants one stack of every inversion buff at once.
public class Playlist : UnderstudyCard
{
    public const string CardId = "TheUnderstudy:Playlist";

    public Playlist() : base(4, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
        WithTip(UnderstudyKeywords.Planned);
        WithTip(typeof(UnweakPower));
        WithTip(typeof(UnvulnerablePower));
        WithTip(typeof(UnshakenPower));
        WithTip(typeof(UnlimitedPower));
        WithTip(typeof(UnjadedPower));
    }

    // Upgrade grants "starts each combat Planned" (see loc's IfUpgraded branch).
    public override bool IsPrePlanned => IsUpgraded;

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var creature = cardPlay.Card.Owner.Creature;
        await PowerCmd.Apply<UnweakPower>(context, creature, 1, creature, cardPlay.Card, false);
        await PowerCmd.Apply<UnvulnerablePower>(context, creature, 1, creature, cardPlay.Card, false);
        await PowerCmd.Apply<UnshakenPower>(context, creature, 1, creature, cardPlay.Card, false);
        await PowerCmd.Apply<UnlimitedPower>(context, creature, 1, creature, cardPlay.Card, false);
        await PowerCmd.Apply<UnjadedPower>(context, creature, 1, creature, cardPlay.Card, false);
    }
}
