namespace Bot.Commands.Markups;

public static class Markup_User {
    // Static
    public static KeyboardButtonRow[] StartMarkupRows => [
        new()
        {
            buttons =
            [
                new KeyboardButtonCallback()
                {
                    text = "Instagram",
                    data = "user_instagram"u8.ToArray() // TODO create callback
                },
                new KeyboardButtonCallback()
                {
                    text = "Twitter",
                    data = "user_twitter"u8.ToArray() // TODO create callback
                }
            ]
        },
        new()
        {
            buttons =
            [
                new KeyboardButtonCallback()
                {
                    text = "Twitch",
                    data = "user_twitch"u8.ToArray() // TODO create callback
                },
                new KeyboardButtonCallback()
                {
                    text = "Telegram",
                    data = "user_telegram"u8.ToArray() // TODO create callback
                }
            ]
        },
        new()
        {
            buttons =
            [
                new KeyboardButtonCallback()
                {
                    text = "TikTok",
                    data = "user_tiktok"u8.ToArray() // TODO create callback
                },
                new KeyboardButtonCallback()
                {
                    text = "YouTube",
                    data = "user_youtube"u8.ToArray() // TODO create callback
                }
            ]
        }
    ]; 
    
    public static ReplyInlineMarkup StartMarkup => new()
    {
        rows = StartMarkupRows
    };
}