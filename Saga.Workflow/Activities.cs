using Temporalio.Activities;

namespace Saga.Worker;

public record TransferDetails(decimal Amount, string FromAmount, string ToAmount, string ReferenceId);

public class Activities
{
    [Activity]
    public async Task Withdraw(TransferDetails d)
    {
        Console.WriteLine($"Withdrawing {d.Amount} from account {d.FromAmount}. ReferenceId: {d.ReferenceId}");
    }

    [Activity]
    public async Task WithdrawCompensation(TransferDetails d)
    {
        Console.WriteLine($"Withdrawing Compensation {d.Amount} from account {d.FromAmount}. ReferenceId: {d.ReferenceId}");
    }

    [Activity]
    public async Task Deposit(TransferDetails d)
    {
        Console.WriteLine($"Depositing {d.Amount} into account {d.ToAmount}. ReferenceId: {d.ReferenceId}");
    }

    [Activity]
    public async Task DepositCompensation(TransferDetails d)
    {
        Console.WriteLine($"Depositing Compensation {d.Amount} int account {d.ToAmount}. ReferenceId: {d.ReferenceId}");
    }

    [Activity]
    public async Task StepWithError(TransferDetails d)
    {
        Console.WriteLine($"Simulate failure to trigger compensation. ReferenceId: {d.ReferenceId}");
        throw new Exception("Simulated failure");
    }
}