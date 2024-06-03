using Bot.Models.Abstractions;

namespace Bot.Models.Users;

public class Task : IEntity<string> {
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletionTime { get; set; }
    
    // Foreign
    public ApplicationUser? User { get; set; }
    public long UserId { get; set; }
}