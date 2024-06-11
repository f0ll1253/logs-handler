using System.Security.Cryptography;
using System.Text;

namespace Bot.Models.Extensions {
	internal static class StringExtensions {
		public static string Sha256(this string str) {
			string hash;

			using (var sha = SHA256.Create()) {
				hash = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(str)));
			}

			return hash;
		}
	}
}