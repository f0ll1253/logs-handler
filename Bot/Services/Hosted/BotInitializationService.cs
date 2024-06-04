using Bot.Bot.Abstractions;

namespace Bot.Services.Hosted {
	public class BotInitializationService(
		Client client,
		IUpdateHandler handler,
		IConfiguration configuration
	) : IHostedService {
		private UpdateManager _manager;

		public async Task StartAsync(CancellationToken cancellationToken) {
			await client.LoginBotIfNeeded(configuration["Bot:Token"]);
			_manager = client.WithUpdateManager(
				async update => {
					try {
						await handler.HandleUpdateAsync(update, _manager.Users);
					} catch (Exception e) {
						await handler.HandleErrorAsync(update, _manager.Users, e);
					}
				},
				Path.Combine(Constants.Directory_Session, "state.json"),
				reentrant: true
			);
		}

		public Task StopAsync(CancellationToken cancellationToken) {
#if RELEASE
        _manager.SaveState(Path.Combine(Directory_Session, "state.json"));
#endif
			return Task.CompletedTask;
		}
	}
}