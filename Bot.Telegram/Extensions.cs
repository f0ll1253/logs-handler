using System.Text;

using TL;

namespace Bot.Telegram {
	internal static class Extensions {
		public static void AppendBlockquote(this StringBuilder builder, string message, string value, ICollection<MessageEntity> entities) {
			builder.AppendLine($"{message}: {value}");
			entities.Add(new MessageEntityBlockquote {
				length = value.Length,
				offset = builder.Length - value.Length - 1
			});
		}
	}
}