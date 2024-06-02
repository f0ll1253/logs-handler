using System.Collections;
using System.Text;

using Bot.Models.Abstractions;

namespace Bot.Services.Hosted;

public class Bootstrapper(
    // Bot
    Client client, 
    IConfiguration configuration,
    
    // UpdateHandler
    IServiceProvider provider,
    ILogger<Bootstrapper> logger) : IHostedService {
    private UpdateManager _manager;
    
    public async Task StartAsync(CancellationToken cancellationToken) {
        await client.LoginBotIfNeeded(configuration["Bot:Token"]);
        _manager = client.WithUpdateManager(
            HandleUpdate,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Session", "state.json"),
            reentrant: true
        );
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        _manager.SaveState(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Session", "state.json"));

        return Task.CompletedTask;
    }
    
    // Update Handler
    public async Task HandleUpdate(Update update) {
        IEnumerable<object> commands = [];
        Update command_update = null;
        string command_trigger = "";
        User user = null;
        
        switch (update) {
            case UpdateNewMessage {message: Message {message: var text, peer_id: var id}} unm:
                if (text is null) {
                    break;
                }
                
                var index = text.IndexOf(' ');
                
                user = _manager.Users[id];
                command_update = unm;
                command_trigger = text[..(index == -1 ? text.Length : index)];
                
                commands = provider.GetKeyedServices<ICommand<UpdateNewMessage>>(command_trigger);
                break;
            case UpdateBotCallbackQuery {data: var data, user_id: var id} ucq:
                var str = Encoding.UTF8.GetString(data);
                
                index = str.IndexOf(':');
                user = _manager.Users[id];
                command_update = ucq;
                command_trigger = str[..(index == -1 ? str.Length : index)];

                commands = provider.GetKeyedServices<ICommand<UpdateBotCallbackQuery>>(command_trigger);
                break;
            default:
                logger.LogWarning($"Command: unknown type '{update.GetType().Name}'");
                return;
        }
        
        if (!commands.Any()) { // TODO add state handler
            logger.LogWarning($"User @{user.username} (#{user.id})");
            logger.LogWarning($"Command: not found '{command_trigger}'");
            return;
        }

        await (Task)GetType()
            .GetMethod("ExecuteCommandsAsync")!
            .MakeGenericMethod(command_update.GetType())
            .Invoke(this, [commands, command_update, user])!;
    }

    public async Task ExecuteCommandsAsync<TUpdate>(IEnumerable commands, Update command_update, User user) where TUpdate : Update {
        foreach (ICommand<TUpdate> command in commands) {
            // if can't execute
            if (command is IFilter<User> filter && !await filter.CanExecuteAsync(user)) {
                logger.LogWarning($"User @{user.username} (#{user.id})");
                logger.LogWarning($"Command: {commands.GetType().Name}");
                logger.LogWarning("State: can't execute");
                return;
            }

            await command.ExecuteAsync((TUpdate)command_update, user);
        }
    }
}