using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MongoDB.Driver;
using MongoDB.Bson;
using SharpCompress.Common;
using Telegram.Bot.Extensions.Polling;
using System.Threading;
using Telegram.Bot.Examples.Echo;

namespace TelegramNoteBot
{
    class Program
    {
        private static string token { get; set; } = "1909541944:AAEVkpxs19CMI3WCy5WT8ruYX6goLeETfNI";
        private static TelegramBotClient client;

        public static void Main(string[] args)
        {
            client = new TelegramBotClient(token);
            using var cts = new CancellationTokenSource();

            MongoClient dbClient = new MongoClient("mongodb+srv://karina:Ff224903@cluster0.gzdmw.mongodb.net/test");
            IMongoDatabase database = dbClient.GetDatabase("TGBotDB");
            IMongoCollection<BsonDocument> notesCollection = database.GetCollection<BsonDocument>("Notes");
            NoteRepository noteRepository = new NoteRepository(notesCollection);

            var handlers = new Handlers(noteRepository);
            client.StartReceiving(new DefaultUpdateHandler(handlers.HandleUpdateAsync, handlers.HandleErrorAsync),
                   cts.Token);

            Console.ReadLine();
            cts.Cancel();

            /*
                        client.StartReceiving();
                        client.OnMessage += TelegramLogic.OnMessageHandler;
                        System.Console.ReadLine();
                        client.StopReceiving();*/
        }

    }
}
