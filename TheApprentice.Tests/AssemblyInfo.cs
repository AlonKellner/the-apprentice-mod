// Disable parallel test execution: game content registries (CustomContentDictionary,
// ModHelper.AddModelToPool) use non-thread-safe Dictionary internally.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
