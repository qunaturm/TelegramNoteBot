using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MongoDB.Driver;
using MongoDB.Bson;
using SharpCompress.Common;

namespace TelegramNoteBot
{
    class Program
    {
        private static string token { get; set; } = "1909541944:AAEVkpxs19CMI3WCy5WT8ruYX6goLeETfNI";
        private static TelegramBotClient client;
/*        static MongoClient dbClient = new MongoClient("mongodb+srv://karina:Ff224903@cluster0.gzdmw.mongodb.net/test");
        static IMongoDatabase database = dbClient.GetDatabase("TGBotDB");
        static IMongoCollection<Note> notesCollection = database.GetCollection<Note>("Notes");*/

        public static void Main(string[] args)
        {
            MongoClient dbClient = new MongoClient("mongodb+srv://karina:Ff224903@cluster0.gzdmw.mongodb.net/test");
            IMongoDatabase database = dbClient.GetDatabase("TGBotDB");
            //IMongoCollection<Note> notesCollection = database.GetCollection<Note>("Notes");

            client = new TelegramBotClient(token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            System.Console.ReadLine();
            client.StopReceiving();
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            var userName = message.From.Username;
            if (message.Text != null)
            {
                switch(message.Text)
                {
                    case "/start":
                        var responseForStart = client.SendTextMessageAsync(message.Chat.Id, 
                            $"Привет, {userName}! Для того, чтобы узнать о возможностях этого бота, нажми кнопку 'Обзор функций'", 
                            replyMarkup: GetButtons());
                        break;

                    case "Обзор функций":
                        var responseForHello = client.SendTextMessageAsync(message.Chat.Id, "Бот позволяет создавать заметки и напоминалки чтобы башка твоя тупая дырявая ничего не забыла", replyMarkup: GetButtons());
                        break;

                    case "Создать заметку":
                        CreateNewNote(e);
                        //var responseForNote = client.SendTextMessageAsync(message.Chat.Id, "щас создадим заметку", replyToMessageId: message.MessageId);
                        break;

                    case "Создать напоминание":
                        var responseForRemind = client.SendTextMessageAsync(message.Chat.Id, "щас создадим напоминание", replyToMessageId: message.MessageId);
                        break;

                    case "aaaAAaaAAAAAa": 
                        var responseForAAA = client.SendTextMessageAsync(message.Chat.Id, "чего орёшь дурик", replyToMessageId: message.MessageId);
                        break;

                    default:
                        await client.SendTextMessageAsync(message.Chat.Id, "не ферштейн я тебя дурика, команду выбери", replyMarkup: GetButtons());
                        break;
                }
        }
    }   

        private static async void CreateNewNote(MessageEventArgs e)
        {
            var message = e.Message;
            client.SendTextMessageAsync(message.Chat.Id, "Отправьте заметку");
            client.StartReceiving();
            client.OnMessage += GetLastMessage;
            client.StopReceiving();
            client.SendTextMessageAsync(message.Chat.Id, "Ваша заметка сохранена!");

        }

        private static void GetLastMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                var message = e.Message;
                var lastMessageId = e.Message.MessageId;
                var newNote = new Note(message.From.Id, message.Text, false);
                BsonDocument doc = new BsonDocument
                {
                    {"userId",  message.From.Id},
                    {"noteId", message.MessageId },
                    { "Text", message.Text},
                    { "isRemind", false}
                };
                MongoClient dbClient = new MongoClient("mongodb+srv://karina:Ff224903@cluster0.gzdmw.mongodb.net/test");
                IMongoDatabase database = dbClient.GetDatabase("TGBotDB");
                IMongoCollection<BsonDocument> notesCollection = database.GetCollection<BsonDocument>("Notes");
                notesCollection.InsertOne(doc);

            }

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
