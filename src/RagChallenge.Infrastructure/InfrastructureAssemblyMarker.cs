// Purpose: Identifies the Infrastructure boundary and its approved inward references for structural verification.
using System.Reflection;

namespace RagChallenge.Infrastructure;

public static class InfrastructureAssemblyMarker
{
    public static IReadOnlyCollection<Assembly> ReferencedCoreAssemblies { get; } =
    [
        typeof(Application.ApplicationAssemblyMarker).Assembly,
        typeof(Domain.DomainAssemblyMarker).Assembly,
    ];
}
