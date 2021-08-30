using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramNoteBot;

namespace Telegram.Bot.Examples.Echo
{
    public class UserInfo
    {
        public long id { get; set; }
        public bool isComand { get; set;  }
    }
    public class Handlers
    {
        private static long noteId = 0;
        private NoteRepository _noteRepository;
        //private ConcurrentDictionary<long, UserState> _usersState;
        private UserInfo _userInfo;
        public Handlers(NoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
            _userInfo = new UserInfo();
        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            _userInfo.id = message.From.Id;
            _userInfo.isComand = true;

            if (_userInfo.isComand == true)
            {
                Console.WriteLine($"Receive message type: {message.Type}");
                if (message.Type != MessageType.Text)
                    return;

                var action = (message.Text.Split(' ').First()) switch
                {
                    "/inline" => SendInlineKeyboard(botClient, message),
                    "/keyboard" => SendReplyKeyboard(botClient, message),
                    "/remove" => RemoveKeyboard(botClient, message),
                    _ => Usage(botClient, message)
                };
                var sentMessage = await action;
                Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");

                async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
                {
                    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Обзор функций", "functionsCallback"),
                            InlineKeyboardButton.WithCallbackData("Создать заметку", "createNotesCallback"),

                    },
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("Мои заметки", "showNotesCallback"),
                            InlineKeyboardButton.WithCallbackData("Ещё что-нибудь", "somethingFuckingElseCallback")
                        },
                });

                    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                text: "Choose",
                                                                replyMarkup: inlineKeyboard);
                }

            }

            async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message)
            {
                var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                    new KeyboardButton[][]
                    {
                        new KeyboardButton[] { "1.1", "1.2" },
                        new KeyboardButton[] { "2.1", "2.2" },
                    })
                {
                    ResizeKeyboard = true
                };

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Choose",
                                                            replyMarkup: replyKeyboardMarkup);
            }

            async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message)
            {
                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: "Removing keyboard",
                                                            replyMarkup: new ReplyKeyboardRemove());
            }

            async Task<Message> Usage(ITelegramBotClient botClient, Message message)
            {
                const string usage = "Usage:\n" +
                                     "/inline   - send inline keyboard\n" +
                                     "/keyboard - send custom keyboard\n" +
                                     "/remove   - remove custom keyboard\n";

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: usage,
                                                            replyMarkup: new ReplyKeyboardRemove());
            }
        }


        private async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var action = (callbackQuery.Data) switch
            {
                "functionsCallback" => TellMeAboutFunctional(),
                "createNotesCallback" => CreateNewNote()
                //_ => botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id, text: "tatat")
            };
            var sentMessage = await action;

            async Task<Message> TellMeAboutFunctional()   //=> botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id, "создание заметок и пока что всё");
            {
                return await botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id, "создание заметок и пока что всё");
            }

            async Task<Message> CreateNewNote()
            {
                _userInfo.isComand = false;
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введите свою заметку", ParseMode.Default, null, false, false, 0, replyMarkup: new ForceReplyMarkup { Selective = true });
                while (true)
                {
                    if (callbackQuery.Message.ReplyToMessage != null && callbackQuery.Message.ReplyToMessage.Text.Contains("Введите свою заметку"))
                    {
                        Note note = new Note(_userInfo.id, ++noteId, callbackQuery.Message.Text, false);
                        _noteRepository.AddNewNote(note);
                        _userInfo.isComand = true;
                        return await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                                                                    text: "Заметка сохранена!");
                    }
                }
                return await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id,
                                                                text: "миша всё хуйня давай по-новой");
            }
        }

        private async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
        {
            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                // displayed result
                new InlineQueryResultArticle(
                    id: "3",
                    title: "TgBots",
                    inputMessageContent: new InputTextMessageContent(
                        "hello"
                    )
                )
            };

            await botClient.AnswerInlineQueryAsync(
                inlineQueryId: inlineQuery.Id,
                results: results,
                isPersonal: true,
                cacheTime: 0);
        }

        private Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
            return Task.CompletedTask;
        }

        private Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }
    }
}
