using Bot.Data;

namespace Bot.Services;

[RegisterScoped]
public class TasksManager(UsersDbContext context, ILogger<TasksManager> logger) {
    public async Task RegisterTask(Task task, long user_id, string name) {
        var active_task = new Models.Users.Task
        {
            Name = name,
            UserId = user_id
        };

        await context.AddAsync(active_task);
        await context.SaveChangesAsync();

         task.ContinueWith((task, state) => _OnComplete(task, (string)state), active_task.Id);
    }

    private async Task _OnComplete(Task completed_task, string id) {
        if (completed_task.IsFaulted) {
            logger.LogError(completed_task.Exception, null);
            
            return;
        }
        
        var task = await context.FindAsync<Models.Users.Task>(id);

        task.IsCompleted = true;
        task.CompletionTime = DateTimeOffset.UtcNow;
        
        context.Update(task);
        await context.SaveChangesAsync();
    }
}