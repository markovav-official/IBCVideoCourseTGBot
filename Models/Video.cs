using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace IBCVideoCourseTGBot.Models;

public class Video : Step
{
    public string Text { get; set; } = null!;
    public string Link { get; set; } = null!;
    public string ButtonText { get; set; } = null!;
    public string ImageText { get; set; } = null!;

    internal override async Task Send(long id, TelegramBotClient bot)
    {
        var image = ImageToTextConverter.ConvertTextToImage(ImageText, textSize: 120, bgColor: "40BA21");

        await bot.SendPhotoAsync(id,
            InputFile.FromStream(image),
            // caption: Text + "\n\nWatch the video 👉 " + Link,
            caption: Text + $"\n\n \ud83d\udc49 {Link}",
            parseMode: ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton(ButtonText)
            {
                CallbackData = Lesson.Id + Id
            }));
    }
}