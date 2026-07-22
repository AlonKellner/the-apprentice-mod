using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Timeline;

namespace TheUnderstudy.TheUnderstudyCode.Timeline;

// Injects The Understudy's epochs + story into the game's compile-time-baked Timeline registries.
//
// EpochModel keeps a hardcoded `_allEpochs` list plus id<->type dictionaries, all built in its static
// constructor from a source-generated subtype list — there is no runtime scan and no BaseLib API, so a
// mod's epoch types are invisible until added here by reflection. We add our 7 epoch types to
// `_allEpochs` + `_epochTypeDictionary` (id->type, used by EpochModel.Get) + `_typeToIdDictionary`
// (type->id, used by GetId), null the cached id list/hashset so they rebuild, and register
// UnderstudyStory in StoryModel's `_storyTypeDictionary` under its slugified id.
//
// Fail-soft: any reflection break just logs and skips, so a future game update that renames these
// internals can never block the mod from loading — the epochs simply won't appear until fixed.
public static class EpochRegistrar
{
    private static bool _registered;

    private static readonly Type[] EpochTypes =
    [
        typeof(Understudy1Epoch), typeof(Understudy2Epoch), typeof(Understudy3Epoch),
        typeof(Understudy4Epoch), typeof(Understudy5Epoch), typeof(Understudy6Epoch),
        typeof(Understudy7Epoch),
    ];

    public static void Register()
    {
        if (_registered) return;
        _registered = true;

        try
        {
            // Touch a static member so EpochModel's static ctor runs and the registries exist.
            _ = EpochModel.AllEpochs;

            var allEpochs = (List<Type>)AccessTools.Field(typeof(EpochModel), "_allEpochs").GetValue(null)!;
            var idToType = (Dictionary<string, Type>)AccessTools.Field(typeof(EpochModel), "_epochTypeDictionary").GetValue(null)!;
            var typeToId = (Dictionary<Type, string>)AccessTools.Field(typeof(EpochModel), "_typeToIdDictionary").GetValue(null)!;

            foreach (var type in EpochTypes)
            {
                if (typeToId.ContainsKey(type)) continue;
                var instance = (EpochModel)Activator.CreateInstance(type)!;
                idToType[instance.Id] = type;
                typeToId[type] = instance.Id;
                if (!allEpochs.Contains(type)) allEpochs.Add(type);
            }

            // Invalidate the cached id list + hashset (AllEpochIds/EpochIdsHashSet rebuild lazily from
            // _allEpochs via GetId, which now resolves our types).
            AccessTools.Field(typeof(EpochModel), "_allEpochIds").SetValue(null, null);
            AccessTools.Field(typeof(EpochModel), "_epochIdsHashSet").SetValue(null, null);

            // Register the story (keyed by StoryModel.Id, which StoryModel.Get looks up after slugifying
            // each epoch's StoryId). Field access triggers StoryModel's static ctor first.
            var storyDict = (Dictionary<string, Type>)AccessTools.Field(typeof(StoryModel), "_storyTypeDictionary").GetValue(null)!;
            storyDict[UnderstudyStory.RegistrationId] = typeof(UnderstudyStory);

            MainFile.Logger.Info($"EpochRegistrar: registered {EpochTypes.Length} Understudy epochs + story into the Timeline.");
        }
        catch (Exception e)
        {
            MainFile.Logger.Error("EpochRegistrar: failed to inject epochs (Timeline story will be absent): " + e);
        }
    }
}
