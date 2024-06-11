using System.Text;

namespace Bot.Extensions {
	internal static class StringExtensions {
		public static byte[] Utf8(this string str) {
			return Encoding.UTF8.GetBytes(str);
		}

		public static string Utf8(this byte[] bytes) {
			return Encoding.UTF8.GetString(bytes);
		}
	}
}