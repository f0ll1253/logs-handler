namespace Bot.Commands.Markups;

public static class Markup_General {
     public static string AvailableLogsText { get; } = "Available logs";
     public static string AvailableLogsError { get; } = "No available logs";
     
    /// <param name="method">Method for processing logs by name</param>
    public static ReplyInlineMarkup? AvailableLogsMarkup(string method, string back_method = "user_start", int page = 0, int limit = 5) {
        var logs = Directory
            // Select logs by creation time
            .GetDirectories(Directory_Extracted)
            .Select(x => new DirectoryInfo(x))
            .OrderByDescending(x => x.CreationTime)
            .Skip(page * limit)
            // Convert to button rows
            .Select(x => new KeyboardButtonRow()
            {
                buttons = [
                    new KeyboardButtonCallback()
                    {
                        text = x.Name,
                        data = $"{method}:{x.Name}:{method}:{back_method}:{page}".Utf8()
                    }
                ]
            })
            .ToList();
        var count = logs.Count;

        if (count == 0) {
            return null;
        }
        
        logs = logs
            .Take(5)
            .ToList();

        if (NavigationRow(method, back_method, count, page, limit) is { } navigation) {
            logs.Add(navigation);
        }
        
        logs.Add(new()
        {
            buttons = [
                new KeyboardButtonCallback()
                {
                    text = "Back",
                    data = back_method.Utf8()
                }
            ]
        });

        return new() { rows = logs.ToArray() };
    }

    public static KeyboardButtonRow? NavigationRow(string method, string back_method, int count, int page, int limit) {
        var row = new KeyboardButtonRow()
        {
            buttons = [
                null,
                null
            ]
        };

        if (page > 0) {
            row.buttons[0] = new KeyboardButtonCallback()
            {
                text = "\u25c0\ufe0f",
                data = $"user_logs_show:{page - 1}:{method}:{back_method}".Utf8()
            };
        }

        if (count - (page + 1) * limit > 0) {
            row.buttons[1] = new KeyboardButtonCallback()
            {
                text = "\u25b6\ufe0f",
                data = $"user_logs_show:{page + 1}:{method}:{back_method}".Utf8()
            };
        }

        return row.buttons.Count(x => x is not null) == 0 ? null : row;
    }
}