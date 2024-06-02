using Bot.Data;
using Bot.Models.Abstractions;

namespace Bot.Models.Base;

public abstract class RoleFilter(UsersDbContext context, string role) : IFilter<User> {
    public async Task<bool> CanExecuteAsync(User obj) {
        if (await context.FindAsync<ApplicationUser>() is { } user) {
            return user.Roles.Contains(role);
        }

        return false;
    }
}