using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Collections.Concurrent;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramNoteBot;

namespace Telegram.Bot.Examples.Echo
{
    public class Handlers
    {
        private NoteRepository _noteRepository;
        private ConcurrentDictionary<long, UserState> _userInfo;
        
        public Handlers(NoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
            _userInfo = new ConcurrentDictionary<long, UserState>();
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
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery),
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
            UserState value;
            if (_userInfo.GetOrAdd(message.From.Id, UserState.Command).Equals(UserState.Command))
            {
                _userInfo.AddOrUpdate(message.From.Id, UserState.Command, (x, y) => UserState.Command);
            }

            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            Task<Message> action;

            if (_userInfo.GetOrAdd(message.From.Id, UserState.Command).Equals(UserState.Command))
            {
                action = (message.Text.Split(' ').First()) switch
                {
                    "/inline" => SendInlineKeyboard(botClient, message),
                    "/remove" => RemoveKeyboard(botClient, message),
                    _ => Usage(botClient, message)
                };
            }
            else
            {
                action = AddNoteToDB(botClient, message);
            }

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
                                     "/remove   - remove custom keyboard\n";

                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text: usage,
                                                            replyMarkup: new ReplyKeyboardRemove());
            }
        }

        private async Task<Message> AddNoteToDB(ITelegramBotClient botClient, Message message)
        {
            Note note = new Note(message.From.Id, message.MessageId, message.Text, false);
            _noteRepository.AddNewNote(note);
            //_userInfo.GetOrAdd(message.From.Id, UserState.Command);
            _userInfo.AddOrUpdate(message.Chat.Id, UserState.Command, (x, y) => UserState.Command);
            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, "заметка создана");
        }

        private async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var action = (callbackQuery.Data) switch
            {
                "functionsCallback" => TellMeAboutFunctional(),
                "createNotesCallback" => CreateNewNote()
            };

            await action;

            async Task<Message> TellMeAboutFunctional()
            {
                return await botClient.SendTextMessageAsync(chatId: callbackQuery.Message.Chat.Id, "создание заметок и пока что всё");
            }

            async Task CreateNewNote()
            {
                _userInfo.AddOrUpdate(callbackQuery.Message.Chat.Id, UserState.Note, (x, y) => UserState.Note);
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введите свою заметку", replyMarkup: new ForceReplyMarkup { Selective = true });

            }

            async Task<Message> GetNotes()
            {
                UserState value;
                if (_userInfo.TryGetValue(callbackQuery.Message.Chat.Id, out value))
                {
                    _noteRepository.GetAllNotes(callbackQuery.Message.Chat.Id);
                }
            }

        }
    }
}