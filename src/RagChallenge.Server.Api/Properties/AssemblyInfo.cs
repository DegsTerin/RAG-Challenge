// Purpose: Exposes setup-host internals only to the integration-test assembly; production consumers remain outside this boundary.
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RagChallenge.IntegrationTests")]
