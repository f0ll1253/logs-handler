using Bot.Models.Abstractions;

namespace Bot.Models;

public class ApplicationUser : IEntity<long> {
    public long Id { get; set; }
    
    // Foreign
    public List<string> Roles { get; set; }

    public static implicit operator ApplicationUser(User user) => new()
    {
        Id = user.id
    };
}