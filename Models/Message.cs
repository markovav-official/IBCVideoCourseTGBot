using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace IBCVideoCourseTGBot.Models;

public class Message : Step
{
    public string Text { get; set; } = null!;
    public string? ImageText { get; set; } = null!;
    public string? ButtonText { get; set; } = null!;

    internal override async Task Send(long id, TelegramBotClient bot)
    {
        if (Lesson.Id == "12")
        {
            await using var db = new DatabaseContext();
            (await db.Users.FirstAsync(u => u.Id == id)).Finished = true;
            await db.SaveChangesAsync();
        }
        
        if (ImageText is not null)
        {
            var image = ImageToTextConverter.ConvertTextToImage(ImageText, textSize: 120, bgColor: "40BA21");
            await bot.SendPhotoAsync(id, InputFile.FromStream(image),
                caption: Text,
                parseMode: ParseMode.Html,
                replyMarkup: ButtonText is null
                    ? null
                    : new InlineKeyboardMarkup(new InlineKeyboardButton(ButtonText)
                    {
                        CallbackData = Lesson.Id + Id
                    })
            );
            return;
        }

        await bot.SendTextMessageAsync(id, text: Text,
            parseMode: ParseMode.Html,
            replyMarkup: ButtonText is null
                ? null
                : new InlineKeyboardMarkup(new InlineKeyboardButton(ButtonText)
                {
                    CallbackData = Lesson.Id + Id
                }));
    }
}