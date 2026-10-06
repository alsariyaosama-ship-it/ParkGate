using Xunit;

// Disable test parallelization in WinForms architecture/UI tests to prevent concurrent access to static WinForms TypeConverters (e.g., KeysConverter)
[assembly: CollectionBehavior(DisableTestParallelization = true)]
