using MegaCrit.Sts2.Core.Timeline;

namespace TheUnderstudy.TheUnderstudyCode.Timeline;

// The story that groups the 7 Understudy epochs. StoryModel.Get looks it up by
// StringHelper.Slugify(epoch.StoryId) == Slugify("TheUnderstudy") == "THE_UNDERSTUDY", so Id must be
// exactly that. EpochRegistrar injects this type into StoryModel._storyTypeDictionary under that key.
// Epochs[] is the narrative (chronological) order 1..7; on-screen placement comes from each epoch's
// own Era/EraPosition, and reveal order from each epoch's unlock condition — all independent.
public sealed class UnderstudyStory : StoryModel
{
    // The key StoryModel.Get resolves us by (== Slugify("TheUnderstudy")); also used by EpochRegistrar
    // to register the type, since the base Id property is protected.
    public const string RegistrationId = "THE_UNDERSTUDY";

    protected override string Id => RegistrationId;

    public override EpochModel[] Epochs =>
    [
        EpochModel.Get<Understudy1Epoch>(),
        EpochModel.Get<Understudy2Epoch>(),
        EpochModel.Get<Understudy3Epoch>(),
        EpochModel.Get<Understudy4Epoch>(),
        EpochModel.Get<Understudy5Epoch>(),
        EpochModel.Get<Understudy6Epoch>(),
        EpochModel.Get<Understudy7Epoch>(),
    ];
}
