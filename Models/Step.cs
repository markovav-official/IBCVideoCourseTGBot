using Telegram.Bot;

namespace IBCVideoCourseTGBot.Models;

public abstract class Step
{
    public string Id { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    internal abstract Task Send(long id, TelegramBotClient bot);
}