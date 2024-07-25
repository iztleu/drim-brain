namespace BlockchainService.Api.Kafka.Events;

public record CryptoDepositCreatedEvent(
    long Id,
    int UserId,
    string Asset,
    decimal Amount,
    string TransactionHash,
    DateTime CreatedAt);
