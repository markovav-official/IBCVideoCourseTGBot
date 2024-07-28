using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = IBCVideoCourseTGBot.Models.User;

namespace IBCVideoCourseTGBot.Commands;

public static class StartCommand
{
    public static async Task TryExecuteStartCommand(this Message message, TelegramBotClient bot)
    {
        if (message.Text == null || !message.Text.StartsWith("/start"))
        {
            return;
        }

        // if (!message.Text.Equals("/start"))
        // {
        //     var target = message.Text[7..];
        //     await bot.SendTextMessageAsync(message.Chat.Id, "Teleport after " + target,
        //         replyMarkup: new InlineKeyboardMarkup(
        //             new InlineKeyboardButton("CLICK")
        //             {
        //                 CallbackData = target
        //             }
        //         ));
        //     return;
        // }
        
        var db = new DatabaseContext();
        
        if (await db.Users.FirstOrDefaultAsync(u => u.Id == message.From!.Id) is null)
        {
            var user = new User
            {
                Id = message.From!.Id,
                Alias = message.From!.Username,
                Email = "noemail@innopolis.university",
                Confirmed = true
            };
            
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
        }

        await JsonsReader.GetCourse()[0].Steps[0].Send(message.From!.Id, bot);
    }
}