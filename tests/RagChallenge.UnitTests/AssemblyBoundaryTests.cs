// Purpose: Verifies canonical core assembly identity and the approved inward Application-to-Domain relationship.
namespace RagChallenge.UnitTests;

public sealed class AssemblyBoundaryTests
{
    [Fact]
    public void CoreAssembliesUseCanonicalNames()
    {
        Assert.Equal(
            "RagChallenge.Domain",
            typeof(Domain.DomainAssemblyMarker).Assembly.GetName().Name);
        Assert.Equal(
            "RagChallenge.Application",
            typeof(Application.ApplicationAssemblyMarker).Assembly.GetName().Name);
    }

    [Fact]
    public void ApplicationMarkerReferencesOnlyTheDomainAssembly()
    {
        var referencedAssembly =
            Application.ApplicationAssemblyMarker.ReferencedDomainAssembly;

        Assert.Same(typeof(Domain.DomainAssemblyMarker).Assembly, referencedAssembly);
    }
}
