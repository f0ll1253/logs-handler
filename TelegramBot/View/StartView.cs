using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBot.Data;
using TelegramBot.Models;
using TelegramBot.Models.Attributes;
using User = TelegramBot.Data.User;

namespace TelegramBot.View;

public class StartView(ITelegramBotClient bot, AppDbContext context) : CommandsView
{
    [Command(Command = "/start")]
    public Task Start(Update update)
    {
        return bot.SendTextMessageAsync(update.Message!.Chat.Id, $"Hello {update.Message.From!.FirstName}! Please send your invite code. (/invite code)");
    }

    [Command(Command = "/invite")]
    public async Task Invite(Update update)
    {
        var codeId = update.Message!.Text!.Split(' ')[1];
        var code = await context.FindAsync<InviteCode>(codeId);

        if (code is not { IsValid: true })
        {
            await bot.SendTextMessageAsync(update.Message!.Chat.Id, "Invite code invalid");
            
            return;
        }

        if (code.Expire <= DateTime.UtcNow.Ticks)
        {
            code.IsValid = false;
            context.Update(code);
            await context.SaveChangesAsync();
            
            await bot.SendTextMessageAsync(update.Message!.Chat.Id, "Invite code invalid");
            
            return;
        }

        await context.AddAsync(new User(update.Message.From!) { IsApproved = true });
        await context.SaveChangesAsync();
        
        await bot.SendTextMessageAsync(update.Message!.Chat.Id, "Invite code was successfully verified");
    }
}