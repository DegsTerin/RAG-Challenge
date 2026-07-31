// Purpose: Records the setup-only composition boundary; it proves inward assembly ownership without enabling product operations.
using System.Reflection;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal sealed record SetupCompositionBoundary(
    Assembly ApplicationAssembly,
    Assembly InfrastructureAssembly);
