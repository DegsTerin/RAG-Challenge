// Purpose: Hosts setup-only composition and dependency-free health endpoints; RAG, persistence, administration, and external access are excluded.
using RagChallenge.Server.Api.OperationsGovernance;

var app = SetupHost.Build(args);
app.Run();

public partial class Program;
