// Purpose: Serialises integration test classes because process-wide SQLite pool cleanup cannot overlap independent fixture lifetimes safely.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
