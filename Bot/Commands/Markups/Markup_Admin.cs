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
                        data = "admin_settings".Utf8() // TODO create callback
                    }
                ]
            })
            .ToArray()
    };
}