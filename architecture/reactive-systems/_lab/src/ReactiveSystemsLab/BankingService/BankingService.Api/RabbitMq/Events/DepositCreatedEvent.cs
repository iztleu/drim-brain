namespace BankingService.Api.RabbitMQ.Events;

public record DepositCreatedEvent(
    int UserId,
    string Asset,
    decimal Amount,
    DateTime CreatedAt);
