using Temporalio.Client;
using Saga.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole().SetMinimumLevel(LogLevel.Information);

var connection = await TemporalConnection.ConnectAsync(new TemporalConnectionOptions("localhost:7233"));
builder.Services.AddSingleton<ITemporalClient>(provider => new TemporalClient(connection, new()
{
    LoggerFactory = provider.GetRequiredService<ILoggerFactory>(),
}));

// start temporal workers
builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapGet("/", async (ITemporalClient client, string? name) =>
{
    // Start a workflow
    var handle = await client.StartWorkflowAsync(
        (Saga.Workflows.SagaWorkflow wf) => wf.RunAsync(new TransferDetails(100, "acc1000", "acc2000", "1324")),
        new(id: "process-transfer-money-90743818", taskQueue: TasksQueue.TransferMoney)
        {
            //RetryPolicy = new()
            //{
            //    InitialInterval = TimeSpan.FromSeconds(15), // first try will occur after 15 seconds
            //    BackoffCoefficient = 2, // double the delay after each retry
            //    MaximumInterval = TimeSpan.FromMinutes(1), // up to a maximum delay of 1 minute
            //    MaximumAttempts = 100 // fail the activity after 100 attempts
            //}
        });

    // Wait for workflow to complete
    var isSuccess = await handle.GetResultAsync();
    if (isSuccess)
        return Results.Ok("Transfer completed successfully");
    else
        return Results.BadRequest("Transfer failed");
});

app.Run();