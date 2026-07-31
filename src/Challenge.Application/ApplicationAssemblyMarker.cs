// Purpose: Identifies the Application boundary and exposes only its approved inward dependency for structural verification.
using System.Reflection;

namespace Challenge.Application;

public static class ApplicationAssemblyMarker
{
    public static Assembly ReferencedDomainAssembly =>
        typeof(Domain.DomainAssemblyMarker).Assembly;
}
