namespace NotificationService.Storage;

public record Notification(
    Guid Id,
    string UserId,
    string Type,
    string Message,
    DateTime CreatedAt);

public interface INotificationStore
{
    void Add(string userId, string type, string message);
    IReadOnlyList<Notification> GetForUser(string userId);
}

public class InMemoryNotificationStore : INotificationStore
{
    private readonly List<Notification> _notifications = new();
    private readonly object _lock = new();

    public void Add(string userId, string type, string message)
    {
        lock (_lock)
        {
            _notifications.Add(new Notification(
                Guid.NewGuid(), userId, type, message, DateTime.UtcNow));
        }
    }

    public IReadOnlyList<Notification> GetForUser(string userId)
    {
        lock (_lock)
        {
            return _notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }
    }
}
