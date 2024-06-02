using System.Text;

namespace Bot.Extensions;

public static class StringExtensions {
    public static byte[] Utf8(this string str) => Encoding.UTF8.GetBytes(str);
    public static string Utf8(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
}