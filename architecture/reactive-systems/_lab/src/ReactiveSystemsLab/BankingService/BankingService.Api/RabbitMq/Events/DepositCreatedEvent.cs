namespace BankingService.Api.RabbitMQ.Events;

public record DepositCreatedEvent(
    long Id,
    int UserId,
    string Asset,
    decimal Amount,
    DateTime CreatedAt);
