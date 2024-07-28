using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace IBCVideoCourseTGBot.Models;

public partial class Email : Step
{
    private static readonly Dictionary<long, string> ConfirmationCodes = new();
    private static readonly Dictionary<long, int> Attempts = new();
    public string Text { get; set; } = null!;
    public string NotAuthorizedText { get; set; } = null!;
    public string ButtonText { get; set; } = null!;
    public string ValidEmail { get; set; } = null!;
    public string InvalidCode { get; set; } = null!;
    public string TooManyAttempts { get; set; } = null!;

    internal override async Task Send(long id, TelegramBotClient bot)
    {
        // Previous version with email check
        // await using var db = new DatabaseContext();
        // var isAuthorized = await db.Users.AnyAsync(u => u.Id == id && u.Confirmed);
        // await bot.SendTextMessageAsync(id,
        //     text: Text + (isAuthorized ? "" : NotAuthorizedText),
        //     parseMode: ParseMode.Html,
        //     replyMarkup: !isAuthorized
        //         ? null
        //         : new InlineKeyboardMarkup(new InlineKeyboardButton(ButtonText)
        //         {
        //             CallbackData = Lesson.Id + Id
        //         })
        // );
        
        // Current version (no email checks)
        await bot.SendTextMessageAsync(id,
            text: Text,
            parseMode: ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton(ButtonText)
            {
                CallbackData = Lesson.Id + Id
            })
        );
    }

    internal async Task TryReadMessage(Telegram.Bot.Types.Message message, TelegramBotClient bot)
    {
        // Check that message is 6 digits
        if (message.Text != null && message.Text.Trim().Length == 6 && message.Text.Trim().All(char.IsDigit))
        {
            if (!ConfirmationCodes.ContainsKey(message.From!.Id) ||
                ConfirmationCodes[message.From.Id] != message.Text.Trim())
            {
                Attempts[message.From.Id] = Attempts.TryGetValue(message.From.Id, out var attempt) ? attempt + 1 : 1;
                if (Attempts[message.From.Id] >= 3)
                {
                    ConfirmationCodes.Remove(message.From.Id);
                    Attempts.Remove(message.From.Id);
                    await bot.SendTextMessageAsync(message.Chat.Id, TooManyAttempts, parseMode: ParseMode.Html);
                    return;
                }

                await bot.SendTextMessageAsync(message.Chat.Id, InvalidCode, parseMode: ParseMode.Html);
                return;
            }

            ConfirmationCodes.Remove(message.From.Id);

            await using var db = new DatabaseContext();
            (await db.Users.FirstAsync(u => u.Id == message.From.Id)).Confirmed = true;
            await db.SaveChangesAsync();

            await (JsonsReader.GetCourse()[0].Steps[1] as Message)?.Send(message.From.Id, bot)!;
            await bot.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            return;
        }

        // email regex @innopolis.university
        if (message.Text != null && InnopolisEmailRegex().IsMatch(message.Text.Trim()))
        {
            await using var db = new DatabaseContext();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == message.From!.Id);
            if (user is null)
            {
                user = new User
                {
                    Id = message.From!.Id,
                    Alias = message.From!.Username,
                    Email = message.Text.Trim(),
                    Confirmed = false
                };
                await db.Users.AddAsync(user);
            }
            else
            {
                user.Alias = message.From!.Username;
                user.Email = message.Text.Trim();
                user.Confirmed = false;
            }

            await db.SaveChangesAsync();

            ConfirmationCodes.Remove(message.From!.Id);
            var code = new Random().Next(100000, 999999).ToString();
            ConfirmationCodes[message.From!.Id] = code;
            var body = await File.ReadAllTextAsync("course/email-confirmation.html");
            body = body.Replace("{{code}}", code);
            body = body.Replace("{{fullname}}", message.From.FirstName + " " + message.From.LastName);
            EmailService.GetInstance().SendEmail(message.Text.Trim(),
                "Log in to the Pro Innopolis University", body);
            Console.WriteLine($"Email with code was sent to {message.Text.Trim()} by request from {message.From.Id} ({message.From.Username})");
            await bot.SendTextMessageAsync(message.Chat.Id, ValidEmail, parseMode: ParseMode.Html);
        }
    }

    // @innopolis.university or @innopolis.ru
    [GeneratedRegex(@"^[\w\d\._-]+@(innopolis\.university|innopolis\.ru)$")]
    private static partial Regex InnopolisEmailRegex();
}