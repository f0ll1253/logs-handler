namespace Server.Models;

public record Cookie(string Domain, IReadOnlyCollection<string> Lines);