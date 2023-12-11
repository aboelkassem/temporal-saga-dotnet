using Temporalio.Client;
using Temporalio.Worker;
using Saga.Workflows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Saga.Worker;
public sealed class Worker : BackgroundService
{
    private readonly ILoggerFactory _loggerFactory;

    public Worker(ILoggerFactory loggerFactory)
    {
        this._loggerFactory = loggerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create an activity instance since we have instance activities. If we had
        // all static activities, we could just reference those directly.
        var activities = new Activities();
        
        using var worker = new TemporalWorker(
            await TemporalClient.ConnectAsync(new()
            {
                TargetHost = "localhost:7233",
                LoggerFactory = _loggerFactory,
            }),
            new TemporalWorkerOptions(taskQueue: TasksQueue.TransferMoney)
                .AddActivity(activities.Withdraw)
                .AddActivity(activities.WithdrawCompensation)
                .AddActivity(activities.Deposit)
                .AddActivity(activities.DepositCompensation)
                .AddActivity(activities.StepWithError)
                .AddWorkflow<SagaWorkflow>()
            );
        
        // Run worker until cancelled
        Console.WriteLine("Running worker");
        try
        {
            await worker.ExecuteAsync(stoppingToken);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine("Worker cancelled");
        }
    }
}