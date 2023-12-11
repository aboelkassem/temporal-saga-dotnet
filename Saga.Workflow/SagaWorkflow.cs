namespace Saga.Workflows;

using Saga.Workflow;
using System;
using Saga.Worker;
using Temporalio.Workflows;


[Workflow]
public class SagaWorkflow
{
    [WorkflowRun]
    public async Task RunAsync(TransferDetails transfer)
    {
        var activities = new Activities();
        var options = new ActivityOptions()
        {
            StartToCloseTimeout = TimeSpan.FromSeconds(90), // schedule a retry if the Activity function doesn't return within 90 seconds
            RetryPolicy = new()
            {
                InitialInterval = TimeSpan.FromSeconds(15), // first try will occur after 15 seconds
                BackoffCoefficient = 1, // double the delay after each retry
                MaximumInterval = TimeSpan.FromMinutes(1), // up to a maximum delay of 1 minute
                MaximumAttempts = 2 // fail the activity after 2 attempts
            }
        };

        var log = new List<string>();
        var saga = new Saga(log);

        try
        {
            await Workflow.ExecuteActivityAsync(
                (Activities act) => act.Withdraw(transfer),
                options);

            saga.AddCompensation(async () => await Workflow.ExecuteActivityAsync(
                                  (Activities act) => act.WithdrawCompensation(transfer),
                                                                   options));

            await Workflow.ExecuteActivityAsync(
                (Activities act) => act.Deposit(transfer),
                options);

            saga.AddCompensation(async () => await Workflow.ExecuteActivityAsync(
                       (Activities act) => act.DepositCompensation(transfer),
                                  options));

            // ----------- Step 3
            await Workflow.ExecuteActivityAsync(
                           (Activities act) => act.StepWithError(transfer),
                                      options);
        }
        catch (Exception ex)
        {
            saga.OnCompensationComplete(async (log) =>
            {
                /* Send "we're sorry, but.." email to customer... */
                log.Add("Done. Compensation complete!");
            });

            saga.OnCompensationError(async (log) =>
            {
                /* Send emails to internal supporting teams */
                log.Add("Done. Compensation unsuccessful... Manual intervention required!");
            });

            await saga.CompensateAsync();
        }
    }
}