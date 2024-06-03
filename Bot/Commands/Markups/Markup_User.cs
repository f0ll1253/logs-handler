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
                    data = $"user_logs_show:0:user_instagram:{back_method}".Utf8()
                },
                new KeyboardButtonCallback()
                {
                    text = "Twitter",
                    data = $"user_logs_show:0:user_twitter:{back_method}".Utf8()
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
                    data = $"user_logs_show:0:user_twitch:{back_method}".Utf8()
                },
                new KeyboardButtonCallback()
                {
                    text = "Telegram",
                    data = $"user_logs_show:0:user_telegram:{back_method}".Utf8()
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
                    data = $"user_logs_show:0:user_tiktok:{back_method}".Utf8()
                },
                new KeyboardButtonCallback()
                {
                    text = "YouTube",
                    data = $"user_logs_show:0:user_youtube:{back_method}".Utf8()
                }
            ]
        }
    ]; 
    
    public static ReplyInlineMarkup StartMarkup(string back_method) => new()
    {
        rows = StartMarkupRows(back_method)
    };
}