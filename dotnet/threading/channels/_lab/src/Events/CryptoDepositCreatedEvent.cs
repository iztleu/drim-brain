namespace Events;

public record CryptoDepositCreatedEvent(int UserId, int AccountId, string Currency, decimal Amount);
