using System.Collections;
using System.Collections.Immutable;

using TL;

namespace Bot.Core.Messages.WTelegram {
	public class UpdateHandlerResponse {
		public User? User { get; set; }
		public ICollection Commands { get; set; } = ImmutableArray<object>.Empty;
	}
}