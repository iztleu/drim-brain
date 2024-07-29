namespace BlockchainService.Api.Kafka.Events;

public record CryptoDepositCreatedEvent(
    string Id,
    int UserId,
    string Asset,
    decimal Amount,
    string TransactionHash,
    DateTime CreatedAt);
