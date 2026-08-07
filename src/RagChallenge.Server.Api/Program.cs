// Purpose: Hosts the bounded public query API and health endpoints; administration and external access remain disabled unless separately composed.
using RagChallenge.Infrastructure.Documents;
using RagChallenge.Server.Api.OperationsGovernance;

if (PdfRenderWorker.IsWorkerMode(args))
{
    return await PdfRenderWorker.RunAsync(
        Console.OpenStandardInput(),
        Console.OpenStandardOutput());
}

if (OneShotAdministrationHost.IsAdministrationMode(args))
{
    return await OneShotAdministrationHost.RunProductionAsync(args);
}

var app = SetupHost.Build(args);
await app.RunAsync();
return 0;

public partial class Program;
