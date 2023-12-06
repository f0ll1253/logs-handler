namespace TelegramBot.Data;

public class InviteCode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Comment { get; set; } = "";
    public long CreatedAt { get; set; } = DateTime.UtcNow.Ticks;
    public long Expire { get; set; }
    public bool IsValid { get; set; } = true;
}