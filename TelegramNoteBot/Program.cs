using MongoDB.Driver;
using TelegramNoteBot.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace TelegramNoteBot
{
    class Program
    {
        public static string botToken = ConfigurationManager.AppSettings["botToken"];
        public static string mongodbConnectionString = ConfigurationManager.AppSettings["mongodbConnectionString"];
        public static Task Main(string[] args) =>
            CreateHostBuilder(args).Build().RunAsync();

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services
                        .AddHostedService<TelegramBotWorker>()
                        .AddSingleton<TelegramLogicHandlers>()
                        .AddSingleton<IMessageProcessing, MessageProcessing>()
                        .AddSingleton<ICallbackProcessing, CallbackProcessing>()
                        .AddSingleton<IUserRepository, UserRepository>()
                        .AddSingleton<INoteRepository, NoteRepository>()
                        .AddSingleton<IMongoClient, MongoClient>(sp => new MongoClient(mongodbConnectionString))
                        .AddSingleton<IMongoDatabase>(sp => sp.GetService<IMongoClient>().GetDatabase("TGBotDB"))
                        .AddSingleton<IMongoCollection<Note>>(sp => sp.GetService<IMongoDatabase>().GetCollection<Note>("Notes"))
                        .AddSingleton<IMongoCollection<User>>(sp => sp.GetService<IMongoDatabase>().GetCollection<User>("Users")));

        public static void Main1(string[] args)
        {
            using var cts = new CancellationTokenSource();

            MongoClient dbClient = new MongoClient(mongodbConnectionString);
            IMongoDatabase database = dbClient.GetDatabase("TGBotDB");
            IMongoCollection<Note> notesCollection = database.GetCollection<Note>("Notes");
            IMongoCollection<User> usersCollection = database.GetCollection<User>("Users");
            INoteRepository noteRepository = new NoteRepository(notesCollection);
            IUserRepository userRepository = new UserRepository(usersCollection);
            ICallbackProcessing callbackProcessing = new CallbackProcessing(userRepository, noteRepository);
            IMessageProcessing messageProcessing = new MessageProcessing(noteRepository, userRepository);
            TelegramLogicHandlers telegramLogicHandlers = new TelegramLogicHandlers(messageProcessing, callbackProcessing);
            TelegramBotWorker worker = new TelegramBotWorker(telegramLogicHandlers);

            Console.ReadLine();
            cts.Cancel();


        }
    }
}
