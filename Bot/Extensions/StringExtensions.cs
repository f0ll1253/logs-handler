using System.Text;

namespace Bot.Extensions;

public static class StringExtensions {
    public static byte[] Utf8(this string str) => Encoding.UTF8.GetBytes(str);
}