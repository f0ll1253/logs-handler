namespace Bot.Commands.Markups;

public static class Markup_Admin {
    public static ReplyInlineMarkup Start => new()
    {
        rows = Markup_User.StartMarkupRows
            .Append(new()
            {
                buttons = [
                    new KeyboardButtonCallback()
                    {
                        text = "Settings",
                        data = "admin_settings"u8.ToArray() // TODO create callback
                    }
                ]
            })
            .ToArray()
    };
}