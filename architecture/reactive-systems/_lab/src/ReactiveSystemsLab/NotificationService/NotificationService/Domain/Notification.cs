namespace NotificationService.Domain;

public class Notification
{
    public long Id { get; set; }

    public required int UserId { get; init; }

    public required string Text { get; init; }

    public required DateTime CreatedAt { get; init; }
}
