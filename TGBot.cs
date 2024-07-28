using IBCVideoCourseTGBot.Commands;
using IBCVideoCourseTGBot.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using User = IBCVideoCourseTGBot.Models.User;

namespace IBCVideoCourseTGBot;

public class TgBot
{
    private readonly TelegramBotClient _bot;

    public TgBot(string? token)
    {
        if (token is null)
        {
            throw new ApplicationException("Error: Telegram token not set; Telegram bot not functional.");
        }

        _bot = new TelegramBotClient(token);

        var me = _bot.GetMeAsync().Result;
        Console.WriteLine("Started bot @" + me.Username);
    }

    public async Task Run()
    {
        using var cts = new CancellationTokenSource();
        var receiverOptions = new ReceiverOptions();
        await _bot.ReceiveAsync(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.CallbackQuery:
                {
                    var query = update.CallbackQuery!.Data;
                    var lessonIndex = int.Parse(query![..2]);
                    var lesson = JsonsReader.GetCourse()[lessonIndex];
                    var stepIndex = int.Parse(query[2..4]);
                    var step = lesson.Steps[stepIndex];

                    try
                    {
                        await DeleteOrEditMessage(update, step, query, cancellationToken);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (step is Question question && query.Length > 4 && query[4..] != "QSTATUS")
                    {
                        var answer = query[4..];
                        await question.AcceptAnswer(Convert.ToInt32(answer), update.CallbackQuery.From.Id, _bot);
                        return;
                    }

                    if (stepIndex >= lesson.Steps.Count - 1)
                    {
                        var nextLesson = JsonsReader.GetCourse()[lessonIndex + 1];
                        await nextLesson.Steps[0].Send(update.CallbackQuery.From.Id, _bot);
                        return;
                    }

                    var nextStep = lesson.Steps[stepIndex + 1];
                    await nextStep.Send(update.CallbackQuery.From.Id, _bot);
                    break;
                }
                case UpdateType.Message:
                {
                    await update.Message!.TryExecuteStartCommand(_bot);

                    await using var db = new DatabaseContext();

                    // Previous version
                    // if (!await db.Users.AnyAsync(u => u.Id == update.Message!.From!.Id && u.Confirmed,
                    //         cancellationToken: cancellationToken))
                    // {
                    //     await (JsonsReader.GetCourse()[0].Steps[0] as Email)?.TryReadMessage(update.Message!, _bot)!;
                    // }

                    // Current version

                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task DeleteOrEditMessage(Update update, Step step, string query, CancellationToken cancellationToken)
    {
        if (step is Question || query[4..] == "QSTATUS")
        {
            if (update.CallbackQuery!.Message!.Date.AddDays(1) > DateTime.Now)
            {
                await _bot.DeleteMessageAsync(update.CallbackQuery!.Message!.Chat.Id,
                    update.CallbackQuery!.Message!.MessageId, cancellationToken: cancellationToken);
            }
            else
            {
                await _bot.EditMessageMediaAsync(
                    update.CallbackQuery!.Message!.Chat.Id,
                    update.CallbackQuery!.Message!.MessageId,
                    new InputMediaPhoto(InputFile.FromStream(
                        ImageToTextConverter.ConvertTextToImage("Message deleted"))
                    )
                    {
                        HasSpoiler = true,
                        Caption = "Message deleted"
                    },
                    replyMarkup: null,
                    cancellationToken: cancellationToken
                );
            }
        }
        else
        {
            await _bot.EditMessageReplyMarkupAsync(update.CallbackQuery!.Message!.Chat.Id,
                update.CallbackQuery.Message.MessageId, replyMarkup: null,
                cancellationToken: cancellationToken);
        }
    }

    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }
}