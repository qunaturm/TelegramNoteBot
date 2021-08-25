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

        public static void Main(string[] args)
        {
            MongoClient dbClient = new MongoClient("mongodb+srv://karina:Ff224903@cluster0.gzdmw.mongodb.net/test");
            IMongoDatabase database = dbClient.GetDatabase("TGBotDB");
            IMongoCollection<BsonDocument> notesCollection = database.GetCollection<BsonDocument>("Notes");
            NoteRepository noteRepository = new NoteRepository(notesCollection);

            client = new TelegramBotClient(token);
            client.StartReceiving();
            client.OnMessage += TelegramLogic.OnMessageHandler;
            System.Console.ReadLine();
            client.StopReceiving();
        }

    }
}
