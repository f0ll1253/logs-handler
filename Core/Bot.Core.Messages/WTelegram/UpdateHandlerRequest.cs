using SlimMessageBus;

using TL;

using WTelegram;

namespace Bot.Core.Messages.WTelegram {
	public class UpdateHandlerRequest : IRequest<UpdateHandlerResponse> {
		public required Update Update { get; set; }
		public required UpdateManager UpdateManager { get; set; }
	}
}