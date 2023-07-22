using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace IBCVideoCourseTGBot.Models;

public class International : Step
{
    public string Text { get; set; } = null!;

    internal override async Task Send(long id, TelegramBotClient bot)
    {
        await bot.SendPhotoAsync(id, InputFile.FromStream(ImageToTextConverter.ConvertTextToImage(Text)),
            protectContent: true,
            allowSendingWithoutReply: false,
            replyMarkup: new InlineKeyboardMarkup(
                new[]
                {
                    new InlineKeyboardButton("Yes")
                    {
                        CallbackData = Lesson.Id + Id
                    },
                    new InlineKeyboardButton("No")
                    {
                        CallbackData = "1105QSTATUS"
                    }
                }
            ));
    }
}