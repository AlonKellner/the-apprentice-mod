// Disable parallel test execution: game content registries (CustomContentDictionary,
// ModHelper.AddModelToPool) use non-thread-safe Dictionary internally.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

// NOTE: ModelDb.Inject(type) (the framework's documented "use in tests" escape hatch for
// CardModifier.AddModifier<T>'s ModelDb lookup) was tried here and reverted: ModelDb.Contains
// becomes globally true for an injected type for the rest of the test process, and
// AbstractModel's constructor unconditionally throws DuplicateModelException whenever
// ModelDb.Contains(GetType()) is true — with no way to distinguish "canonical" from "standalone
// test instance" intent. That broke every other test that does `new ExpendModifier()` /
// `new PlannedModifier()` etc. to unit-test a modifier's own logic in isolation (see
// ExpendModifierTests, PlannedModifierTests, DreamyModifierTests, AmbitousModifierTests) — a
// long-established, deliberate pattern. So: AddModifier<T>(card) (generic) is the only correct
// form in production code, but its ModelDb dependency makes it untestable in this bare harness;
// that specific code path is verified by reading the game's own log output instead.
