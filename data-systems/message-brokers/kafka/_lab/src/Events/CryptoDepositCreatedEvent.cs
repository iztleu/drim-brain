namespace Events;

public record CryptoDepositCreatedEvent(
    int Id,
    int UserId,
    string AccountNumber,
    string Currency,
    decimal Amount,
    string SourceCryptoAddress,
    string TxId,
    DateTime CreatedAt);
