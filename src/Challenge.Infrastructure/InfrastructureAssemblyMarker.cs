// Purpose: Identifies the Infrastructure boundary and its approved inward references without selecting any concrete adapter.
using System.Reflection;

namespace Challenge.Infrastructure;

public static class InfrastructureAssemblyMarker
{
    public static IReadOnlyCollection<Assembly> ReferencedCoreAssemblies { get; } =
    [
        typeof(Application.ApplicationAssemblyMarker).Assembly,
        typeof(Domain.DomainAssemblyMarker).Assembly,
    ];
}
