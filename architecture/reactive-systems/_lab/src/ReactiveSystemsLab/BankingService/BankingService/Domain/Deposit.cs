namespace BankingService.Domain;

public class Deposit
{
    public long Id { get; set; }

    public required long SourceId { get; init; }

    public required int UserId { get; init; }

    public required string Asset { get; init; }

    public required decimal Amount { get; init; }

    public required DateTime CreatedAt { get; init; }
}
