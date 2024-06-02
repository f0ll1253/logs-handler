namespace Bot.Commands.Markups;

public static class Markup_User {
    public static KeyboardButtonRow[] StartMarkupRows(string back_method) => [
        new()
        {
            buttons =
            [
                new KeyboardButtonCallback()
                {
                    text = "Instagram",
                    data = $"user_instagram:0:{back_method}".Utf8()
                },
                new KeyboardButtonCallback()
                {
                    text = "Twitter",
                    data = $"user_twitter:0:{back_method}".Utf8()
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
                    data = $"user_twitch:0:{back_method}".Utf8()
                },
                new KeyboardButtonCallback()
                {
                    text = "Telegram",
                    data = $"user_telegram:0:{back_method}".Utf8()
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
                    data = $"user_tiktok:0:{back_method}".Utf8()
                },
                new KeyboardButtonCallback()
                {
                    text = "YouTube",
                    data = $"user_youtube:0:{back_method}".Utf8()
                }
            ]
        }
    ]; 
    
    public static ReplyInlineMarkup StartMarkup(string back_method) => new()
    {
        rows = StartMarkupRows(back_method)
    };
}