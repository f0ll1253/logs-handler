using System.Text;

namespace Bot.Telegram {
	internal static class Extensions {
		public static (string name, string path) GetPath(this IConfiguration config, byte[] bytes, Paths type) {
			var name = Encoding.UTF8.GetString(bytes[1..]);

			return (name, Path.Combine(config["Files:Root"]!, type.ToString(), name));
		}
	}
	
	public enum Paths {
		Extracted,
		Downloaded
	}
}