namespace Bot.Commands.Markups;

public static class Markup_General {
     public static string AvailableLogsText { get; } = "Available logs";
     public static string AvailableLogsError { get; } = "No available logs";
    
    /// <param name="showing_method">Callback which shows logs</param>
    /// <param name="method">Method for processing logs by name</param>
    public static ReplyInlineMarkup? AvailableLogsMarkup(string showing_method, string method, string role = "user", int page = 0, int limit = 5) {
        var logs = Directory
            // Select logs by creation time
            .GetDirectories(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Extracted"))
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
                        data = $"{role}_{method}:{x.Name}".Utf8()
                    }
                ]
            })
            .ToList();

        if (logs.Count == 0) {
            return null;
        }

        if (NavigationRow(showing_method, role, logs.Count, page, limit) is { } navigation) {
            logs.Add(navigation);
        }
        
        logs.Add(new()
        {
            buttons = [
                new KeyboardButtonCallback()
                {
                    text = "Back",
                    data = $"{role}_start".Utf8()
                }
            ]
        });

        return new() { rows = logs.ToArray() };
    }

    public static KeyboardButtonRow? NavigationRow(string action, string role, int count, int page, int limit) {
        var row = new KeyboardButtonRow()
        {
            buttons = [
                null,
                null
            ]
        };

        if (page == 0) {
            row.buttons[0] = new KeyboardButtonCallback()
            {
                text = "\u25c0\ufe0f",
                data = $"{role}_{action}:{page - 1}".Utf8()
            };
        }

        if (count - (page + 1) * limit <= 0) {
            row.buttons[1] = new KeyboardButtonCallback()
            {
                text = "\u25b6\ufe0f",
                data = $"{role}_{action}:{page + 1}".Utf8()
            };
        }

        return row.buttons.Count(x => x is not null) == 0 ? null : row;
    }
}