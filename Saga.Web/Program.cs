using Temporalio.Client;
using Saga.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole().SetMinimumLevel(LogLevel.Information);

builder.Services.AddSingleton(ctx =>
    TemporalClient.ConnectAsync(new()
    {
        TargetHost = "localhost:7233",
        LoggerFactory = ctx.GetRequiredService<ILoggerFactory>(),
    }));

// start temporal workers
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapGet("/", async (Task<TemporalClient> clientTask, string? name) =>
{
    
    var client = await clientTask;

    // Start a workflow
    var handle = await client.StartWorkflowAsync(
        (Saga.Workflows.SagaWorkflow wf) => wf.RunAsync(new TransferDetails(100, "acc1000", "acc2000", "1324")),
        new(id: "process-order-number-90743818", taskQueue: TasksQueue.TransferMoney)
        {
            //RetryPolicy = new()
            //{
            //    InitialInterval = TimeSpan.FromSeconds(15), // first try will occur after 15 seconds
            //    BackoffCoefficient = 2, // double the delay after each retry
            //    MaximumInterval = TimeSpan.FromMinutes(1), // up to a maximum delay of 1 minute
            //    MaximumAttempts = 100 // fail the activity after 100 attempts
            //}
        });

    return "Workflow done";
});

app.Run();