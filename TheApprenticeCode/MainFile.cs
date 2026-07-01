using BaseLib.Extensions;
using BaseLib.Patches.Localization;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using TheApprentice.TheApprenticeCode.Cards;
using TheApprentice.TheApprenticeCode.Cards.Modifiers;

namespace TheApprentice.TheApprenticeCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "TheApprentice";
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);
        harmony.PatchAll();

        DescriptionOverrides.CustomizeDescriptionPost += (CardModel card, Creature? target, ref string description) =>
            AppendPlannedFallback(card, ref description);
    }

    // Cards with IsPrePlanned (Signature, Prelude) should always show a "Planned" indicator
    // at the bottom of their description, even outside combat where PlannedModifier hasn't
    // been attached yet (it's only attached via ApprenticeCard.BeforeCombatStart once a
    // combat actually starts). Once the real modifier IS attached, its own
    // ModifyDescriptionPost (registered the same way, via the same CustomizeDescriptionPost
    // event) takes over and shows the dynamic "Planned #N" index instead — this is purely a
    // fallback for when that modifier is absent, so the two never both fire for the same card.
    public static void AppendPlannedFallback(CardModel card, ref string description)
    {
        if (card is ApprenticeCard { IsPrePlanned: true } apprenticeCard
            && !apprenticeCard.TryGetModifier<PlannedModifier>(out _))
            description += "\n[gold]Planned[/gold].";
    }
}
