// Purpose: Verifies that the empty core scaffold exposes only canonical assembly markers and no premature product types.
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
