using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace IBCVideoCourseTGBot.Commands;

public static class StartCommand
{
    public static async Task TryExecuteStartCommand(this Message message, TelegramBotClient bot)
    {
        if (message.Text == null || !message.Text.StartsWith("/start"))
        {
            return;
        }

        if (!message.Text.Equals("/start"))
        {
            var target = message.Text[7..];
            await bot.SendTextMessageAsync(message.Chat.Id, "Teleport after " + target,
                replyMarkup: new InlineKeyboardMarkup(
                    new InlineKeyboardButton("CLICK")
                    {
                        CallbackData = target
                    }
                ));
            return;
        }

        await JsonsReader.GetCourse()[0].Steps[0].Send(message.From!.Id, bot);
    }
}