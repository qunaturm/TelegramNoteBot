using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MongoDB.Driver;
using MongoDB.Bson;
using SharpCompress.Common;
using Newtonsoft.Json.Linq;

namespace TelegramNoteBot
{
    public class TelegramLogic
    {
        private static TelegramBotClient _client;
        TelegramLogic(TelegramBotClient client)
        {
            _client = client;
        }
        public static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            var userName = message.From.Username;
            if (message.Text != null)
            {
                switch (message.Text)
                {
                    case "/start":
                        var responseForStart = _client.SendTextMessageAsync(message.Chat.Id,
                            $"Привет, {userName}! Для того, чтобы узнать о возможностях этого бота, нажми кнопку 'Обзор функций'",
                            replyMarkup: GetButtons());
                        break;

                    case "Обзор функций":
                        var responseForHello = _client.SendTextMessageAsync(message.Chat.Id, "Бот позволяет создавать заметки и напоминалки чтобы башка твоя тупая дырявая ничего не забыла", replyMarkup: GetButtons());
                        break;

                    case "Создать заметку":
                        CreateNewNote(e);
                        //var responseForNote = client.SendTextMessageAsync(message.Chat.Id, "щас создадим заметку", replyToMessageId: message.MessageId);
                        break;

                    case "Создать напоминание":
                        var responseForRemind = _client.SendTextMessageAsync(message.Chat.Id, "щас создадим напоминание", replyToMessageId: message.MessageId);
                        break;

                    case "aaaAAaaAAAAAa":
                        var responseForAAA = _client.SendTextMessageAsync(message.Chat.Id, "чего орёшь дурик", replyToMessageId: message.MessageId);
                        break;

                    default:
                        await _client.SendTextMessageAsync(message.Chat.Id, "не ферштейн я тебя дурика, команду выбери", replyMarkup: GetButtons());
                        break;
                }
            }
        }

        private static async void CreateNewNote(MessageEventArgs e)
        {
            var message = e.Message;
            _client.SendTextMessageAsync(message.Chat.Id, "Отправьте заметку");
            _client.StartReceiving();
            _client.OnMessage += GetLastMessage;
            _client.StopReceiving();
            _client.SendTextMessageAsync(message.Chat.Id, "Ваша заметка сохранена!");

        }

        private static void GetLastMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            var lastMessageId = e.Message.MessageId;
            Note newNote = new Note(message.From.Id, message.Text, false);
            
        }

        private static IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup
            {
                Keyboard = new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>{new KeyboardButton { Text = "Обзор функций"}, new KeyboardButton { Text = "Создать заметку"} },
                    new List<KeyboardButton> { new KeyboardButton { Text = "Создать напоминание" }, new KeyboardButton { Text = "aaaAAaaAAAAAa" } }
                }
            };
        }
    }
}
