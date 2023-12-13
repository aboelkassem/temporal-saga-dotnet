using Temporalio.Activities;

namespace Saga.Worker;

public record TransferDetails(decimal Amount, string FromAmount, string ToAmount, string ReferenceId);

public static class Activities
{
    [Activity]
    public static async Task<bool> Withdraw(TransferDetails d)
    {
        // bool = isWithdrawalSuccessful
        Console.WriteLine($"Withdrawing {d.Amount} from account {d.FromAmount}. ReferenceId: {d.ReferenceId}");
        return true;
    }

    [Activity]
    public static async Task WithdrawCompensation(TransferDetails d)
    {
        Console.WriteLine($"Withdrawing Compensation {d.Amount} from account {d.FromAmount}. ReferenceId: {d.ReferenceId}");
    }

    [Activity]
    public static async Task<bool> Deposit(TransferDetails d)
    {
        // bool = isDepositSuccessful
        Console.WriteLine($"Depositing {d.Amount} into account {d.ToAmount}. ReferenceId: {d.ReferenceId}");
        return true;
    }

    [Activity]
    public static async Task DepositCompensation(TransferDetails d)
    {
        Console.WriteLine($"Depositing Compensation {d.Amount} int account {d.ToAmount}. ReferenceId: {d.ReferenceId}");
    }

    [Activity]
    public static async Task StepWithError(TransferDetails d)
    {
        Console.WriteLine($"Simulate failure to trigger compensation. ReferenceId: {d.ReferenceId}");
        throw new Exception("Simulated failure");
    }
}