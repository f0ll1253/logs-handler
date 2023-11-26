namespace Console.Extensions;

public static class ConsoleExtensions
{
    public static void WriteInvalidLine(this TextWriter writer, string text)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        writer.WriteLine(text);
        System.Console.ForegroundColor = ConsoleColor.White;
    }
    
    public static void WriteValidLine(this TextWriter writer, string text)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        writer.WriteLine(text);
        System.Console.ForegroundColor = ConsoleColor.White;
    }
}