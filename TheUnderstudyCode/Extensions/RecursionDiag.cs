using System;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Logging;

namespace TheUnderstudy.TheUnderstudyCode.Extensions;

// TEMPORARY diagnostic for the stack-overflow crash seen during Planned/Unplayable-heavy resolution
// (Workshop resolving a queue while Da Capo re-queues). Instrumented at the entry of a few suspect methods.
// It catches a runaway two ways:
//   1. RuntimeHelpers.EnsureSufficientExecutionStack() — the robust one: throws a CATCHABLE
//      InsufficientExecutionStackException the moment the real managed stack is nearly exhausted, no matter
//      which methods make up the cycle (works even when the recursion lives in base-game render/preview
//      code that merely calls back into one instrumented mod method as a leaf).
//   2. A [ThreadStatic] depth counter, as a backstop for a cycle that stays entirely inside instrumented
//      mod methods.
// On the first trip it logs the full managed stack trace (which names every method in the cycle) and tells
// the caller to SHORT-CIRCUIT — averting the real StackOverflowException, which .NET cannot catch and which
// hard-crashes the process before it can log anything. So this both surfaces the culprit AND keeps the game
// playable. Remove once the cycle is fixed.
//
// Usage at the very top of a suspected-recursive method:
//     if (!RecursionDiag.Enter("Where")) return <safe fallback>;   // NOTE: on false, do NOT call Leave()
//     try { ...real body... } finally { RecursionDiag.Leave(); }
public static class RecursionDiag
{
    [ThreadStatic] private static int _depth;
    private static bool _dumped;

    private const int DepthThreshold = 400;

    public static bool Enter(string where)
    {
        _depth++;

        bool stackLow = false;
        try { RuntimeHelpers.EnsureSufficientExecutionStack(); }
        catch (InsufficientExecutionStackException) { stackLow = true; }

        if (stackLow || _depth >= DepthThreshold)
        {
            Dump(where, stackLow);
            _depth--; // don't count the short-circuited call
            return false;
        }
        return true;
    }

    public static void Leave() => _depth--;

    private static void Dump(string where, bool stackLow)
    {
        if (_dumped) return;
        _dumped = true;
        Log.Error($"[RecursionDiag] runaway recursion at {where} " +
                  $"(stackNearlyExhausted={stackLow}, depth={_depth}). Short-circuiting to avert a " +
                  $"stack-overflow crash.\nSTACK:\n{Environment.StackTrace}");
    }
}
