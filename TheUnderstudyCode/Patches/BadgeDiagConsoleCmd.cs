using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace TheUnderstudy.TheUnderstudyCode.Patches;

// Dev-console control for the Planned selection index-badge diagnostics (SelectionIndexBadge.Diagnostics).
// The base DevConsole discovers this automatically via ReflectionHelper.GetSubtypesInMods<AbstractConsoleCmd>().
// DebugOnly=false so it registers whenever the console is available, not only in debug builds.
//
//   understudy_badgediag on      -> verbose per-render/clear logging
//   understudy_badgediag off     -> quiet (the always-on stray-badge tripwire still fires)
//   understudy_badgediag dump    -> log the currently tracked badges right now
//   understudy_badgediag         -> toggle
//
// Note: the stray-badge invariant in SelectionIndexBadge.DiagAfterRender logs an error automatically even
// with diagnostics OFF, so the deselect bug is caught in the log without needing this command at all.
public class BadgeDiagConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "understudy_badgediag";
    public override string Args => "[on|off|dump]";
    public override string Description =>
        "Toggle/dump The Understudy's Planned selection index-badge diagnostics.";
    public override bool IsNetworked => false;
    public override bool DebugOnly => false;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        string sub = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "toggle";
        switch (sub)
        {
            case "on": SelectionIndexBadge.Diagnostics = true; break;
            case "off": SelectionIndexBadge.Diagnostics = false; break;
            case "dump":
                SelectionIndexBadge.DumpState("console");
                return new CmdResult(success: true, "Dumped tracked badge state to the log.");
            case "toggle": SelectionIndexBadge.Diagnostics = !SelectionIndexBadge.Diagnostics; break;
            default:
                return new CmdResult(success: false, "Usage: understudy_badgediag [on|off|dump]");
        }

        return new CmdResult(success: true,
            $"Understudy badge diagnostics {(SelectionIndexBadge.Diagnostics ? "ON" : "OFF")}.");
    }
}
