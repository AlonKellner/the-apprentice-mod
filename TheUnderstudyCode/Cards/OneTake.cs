using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheUnderstudy.TheUnderstudyCode.Cards.Powers;
using UnderstudyCharacter = TheUnderstudy.TheUnderstudyCode.Character.TheUnderstudy;

namespace TheUnderstudy.TheUnderstudyCode.Cards;

public class OneTake : UnderstudyCard, ITomeCard
{
    public const string CardId = "TheUnderstudy:OneTake";

    // Dusty Tome (Darv) grants this ancient power. BaseLib's DustyTomePatch selects the Tome card from
    // the character's ITomeCard cards (found in ModelDb.AllCards) and skips the base DustyTome scan —
    // which is required because a CustomCardPoolModel's card set does not surface Ancient-rarity cards,
    // so the base scan finds nothing and crashes (AncientCard null). TomeCharacter is given explicitly
    // for the same reason: the default auto-detect walks CardPool.AllCardIds, which also omits Ancients.
    public CharacterModel TomeCharacter => ModelDb.Character<UnderstudyCharacter>();

    // Ancient rarity: One Take is the Understudy's build-defining engine power (global -1 cost with the
    // Unplayable drawback + its Balanced/Muscle Memory support package). Like base-game
    // ancient powers it leaves the normal reward pool and is granted only by Darv's Dusty Tome (always
    // upgraded).
    public OneTake() : base(3, CardType.Power, CardRarity.Ancient, TargetType.None)
    {
        WithPowerNoTip<OneTakePower>(1);
        WithTip(CardKeyword.Unplayable);
        WithKeyword(CardKeyword.Innate, ConstructedCardModel.UpgradeType.Add);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await CommonActions.Apply<OneTakePower>(context, cardPlay.Card.Owner.Creature, this);
    }
}
