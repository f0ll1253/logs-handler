using System.Security.Cryptography;
using System.Text;

namespace Bot.Extensions {
	public static class StringExtensions {
		public static byte[] Utf8(this string str) {
			return Encoding.UTF8.GetBytes(str);
		}

		public static string Utf8(this byte[] bytes) {
			return Encoding.UTF8.GetString(bytes);
		}

		public static string Sha256(this string str) {
			string hash;

			using (var sha = SHA256.Create()) {
				hash = Convert.ToHexString(sha.ComputeHash(str.Utf8()));
			}

			return hash;
		}
	}
}