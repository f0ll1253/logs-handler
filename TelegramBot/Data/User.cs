namespace TelegramBot.Data;

public class User : TL.User
{
    public bool IsApproved { get; set; }
}