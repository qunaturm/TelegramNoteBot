using MongoDB.Driver;
using TelegramNoteBot.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TelegramNoteBot
{
    class Program
    {
        public static Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            return CreateHostBuilder(args, configuration).Build().RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services
                        .AddHostedService<TelegramBotWorker>()
                        .AddSingleton<IConfiguration>(configuration)
                        .AddSingleton<TelegramLogicHandlers>()
                        .AddSingleton<IMessageProcessing, MessageProcessing>()
                        .AddSingleton<ICallbackProcessing, CallbackProcessing>()
                        .AddSingleton<IUserRepository, UserRepository>()
                        .AddSingleton<INoteRepository, NoteRepository>()
                        .AddSingleton<IMongoClient, MongoClient>(sp => new MongoClient(sp.GetService<IConfiguration>().GetValue<string>("MongoDbConnectionString")))
                        .AddSingleton<IMongoDatabase>(sp => sp.GetService<IMongoClient>().GetDatabase("TGBotDB"))
                        .AddSingleton<IMongoCollection<Note>>(sp => sp.GetService<IMongoDatabase>().GetCollection<Note>("Notes"))
                        .AddSingleton<IMongoCollection<User>>(sp => sp.GetService<IMongoDatabase>().GetCollection<User>("Users")));

    }
}
