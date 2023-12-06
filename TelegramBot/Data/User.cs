namespace TelegramBot.Data;

public class User : Telegram.Bot.Types.User
{
    public User() { }

    public User(Telegram.Bot.Types.User user)
    {
        Id = user.Id;
        IsBot = user.IsBot;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Username = user.Username;
        LanguageCode = user.LanguageCode;
        IsPremium = user.IsPremium;
        AddedToAttachmentMenu = user.AddedToAttachmentMenu;
        CanJoinGroups = user.CanJoinGroups;
        CanReadAllGroupMessages = user.CanReadAllGroupMessages;
        SupportsInlineQueries = user.SupportsInlineQueries;
    }

    public bool IsApproved { get; set; }
}