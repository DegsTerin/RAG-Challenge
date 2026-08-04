// Purpose: Hosts the bounded public query API and health endpoints; administration and external access remain disabled unless separately composed.
using RagChallenge.Server.Api.OperationsGovernance;

var app = SetupHost.Build(args);
app.Run();

public partial class Program;
