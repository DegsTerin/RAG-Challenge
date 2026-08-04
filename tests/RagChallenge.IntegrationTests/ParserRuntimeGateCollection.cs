// Purpose: Serialises the parser assembly-loading gate so concurrently loaded integration dependencies cannot contaminate its process-local evidence.
namespace RagChallenge.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ParserRuntimeGateSerialisation
{
    public const string Name = "Parser runtime gate";
}
