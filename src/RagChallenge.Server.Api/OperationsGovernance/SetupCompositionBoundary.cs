// Purpose: Records the approved host composition boundary for structural verification without exposing implementation services.
using System.Reflection;

namespace RagChallenge.Server.Api.OperationsGovernance;

internal sealed record SetupCompositionBoundary(
    Assembly ApplicationAssembly,
    Assembly InfrastructureAssembly);
