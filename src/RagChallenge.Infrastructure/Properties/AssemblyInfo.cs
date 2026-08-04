// Purpose: Exposes persistence internals only to the integration-test assembly so migration upgrades can be verified without widening production contracts.
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RagChallenge.IntegrationTests")]
