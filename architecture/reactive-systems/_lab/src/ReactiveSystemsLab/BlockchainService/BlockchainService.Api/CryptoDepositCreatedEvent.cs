namespace BlockchainService.Api;

public class CryptoDepositCreatedEvent
{
    public required long Id { get; init; }
    public required int UserId { get; init; }
    public required string Asset { get; init; }
    public required decimal Amount { get; init; }
    public required string TransactionHash { get; init; }
    public required DateTime CreatedAt { get; init; }
}
