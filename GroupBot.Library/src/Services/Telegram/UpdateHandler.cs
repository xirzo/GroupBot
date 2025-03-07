﻿using GroupBot.Library.Commands;
using GroupBot.Library.Commands.Parser;
using GroupBot.Library.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Library.Services.Telegram;

public class UpdateHandler : IUpdateHandler
{
    private readonly CommandParser _parser;
    private readonly ILogger _logger;

    public UpdateHandler(CommandParser parser, ILogger logger)
    {
        _parser = parser;
        _logger = logger;
    }

    public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message is null || update.Message.From is null || string.IsNullOrEmpty(update.Message.Text) || update.Message.Text[0] != '/')
        {
            return Task.CompletedTask;
        }

        var parseResult = _parser.Parse(update.Message.Text);

        if (!parseResult.Success)
        {
            _logger.Error(parseResult.ErrorMessage);

            botClient.SendMessage(
                 chatId: update.Message.Chat.Id,
                 text: parseResult.ErrorMessage,
                 replyParameters: new ReplyParameters { MessageId = update.Message.MessageId }, cancellationToken: cancellationToken);

            return Task.CompletedTask;
        }

        var validated = ValidatedMessage.FromTelegramMessage(update.Message);

        if (validated == null)
        {
            _logger.Error($"Cannot validate the message {update.Message}");
            return Task.CompletedTask;
        }

        return parseResult.Command.Execute(validated, botClient, parseResult.Parameters);
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        _logger.Error($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}
