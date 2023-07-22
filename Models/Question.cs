using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace IBCVideoCourseTGBot.Models;

public class Question : Step
{
    public string Text { get; set; } = null!;
    public string[] Options { get; set; } = null!;
    public int Correct { get; set; }
    private class OrderedOption
    {
        public string Text { get; set; } = null!;
        public int Index { get; set; }
    }

    private List<OrderedOption> GetShuffledOrderedOptions()
    {
        var rnd = new Random();
        return Options.Select((option, index) => new OrderedOption
        {
            Text = option,
            Index = index
        }).OrderBy(_ => rnd.Next()).ToList();
    }

    internal override async Task Send(long id, TelegramBotClient bot)
    {
        var optionsInButtons = Options.All(o => o.Length <= 32);
        var options = GetShuffledOrderedOptions();
        await bot.SendPhotoAsync(id, InputFile.FromStream(ImageToTextConverter.ConvertTextToImage(Text)),
            caption: optionsInButtons
                ? null
                : string.Join("\n", options.Select((option, displayIndex) => $"{displayIndex + 1} - {option.Text}")),
            protectContent: true,
            allowSendingWithoutReply: false,
            replyMarkup: optionsInButtons
                ? new InlineKeyboardMarkup(options.Select(option =>
                    new[]
                    {
                        new InlineKeyboardButton(option.Text)
                        {
                            CallbackData = Lesson.Id + Id + option.Index
                        }
                    }))
                : new InlineKeyboardMarkup(options.Select((option, displayIndex) =>
                    new InlineKeyboardButton((displayIndex + 1).ToString())
                    {
                        CallbackData = Lesson.Id + Id + option.Index
                    }
                )));
    }

    internal async Task AcceptAnswer(int answer, long id, TelegramBotClient bot)
    {
        if (answer == Correct)
        {
            var successMessage = JsonsReader.GetSuccessMessages().GetRandom();
            var successImage = ImageToTextConverter.ConvertTextToImage("Correct", textSize: 120, bgColor: "40BA21");
            await bot.SendPhotoAsync(id, InputFile.FromStream(successImage),
                caption: successMessage.Message,
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton(successMessage.ButtonText)
                {
                    CallbackData = Lesson.Id + Id + "QSTATUS"
                }));
        }
        else
        {
            var wrongMessage = JsonsReader.GetWrongMessages().GetRandom();
            var wrongImage =
                ImageToTextConverter.ConvertTextToImage("Wrong", textSize: 120, bgColor: "FF2B2B");
            await bot.SendPhotoAsync(id, InputFile.FromStream(wrongImage),
                caption: wrongMessage.Message,
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton(wrongMessage.ButtonText)
                {
                    CallbackData = Lesson.Id + (Convert.ToInt32(Id) - 1).ToString().PadLeft(2, '0') + "QSTATUS"
                }));
        }
    }
}