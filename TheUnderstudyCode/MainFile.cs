using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using TheUnderstudy.TheUnderstudyCode.Timeline;

namespace TheUnderstudy.TheUnderstudyCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "TheUnderstudy";
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        // Inject the Timeline story/epochs into the game's static registries before patching, so the
        // reachability + reveal patches can resolve our epoch types (see EpochRegistrar).
        EpochRegistrar.Register();

        Harmony harmony = new(ModId);
        harmony.PatchAll();
    }
}
