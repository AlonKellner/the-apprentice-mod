namespace TheUnderstudy.TheUnderstudyCode.Relics;

// The pure study-count arithmetic for the Chaotic Book, factored out of the relic so it is unit-
// testable without the game (the relic itself calls Log.* and RelicCmd, which the bare test host
// cannot run). Studying the book at a Rest Site advances the count; at the threshold it transforms.
public static class StudyProgress
{
    // How many studies remain before the book transforms (never negative).
    public static int Remaining(int studyCount, int required) =>
        studyCount >= required ? 0 : required - studyCount;

    // Whether enough studies have accumulated to transform into the Book of Order.
    public static bool IsComplete(int studyCount, int required) => studyCount >= required;
}
