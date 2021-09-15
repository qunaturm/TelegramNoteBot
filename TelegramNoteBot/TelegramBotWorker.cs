using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using TelegramNoteBot.Handlers;

namespace TelegramNoteBot
{
    public class TelegramBotWorker : BackgroundService
    {
        private readonly TelegramLogicHandlers _telegramLogicHandlers;
        private readonly IConfiguration _configuration;
        private readonly string _botToken;
        public TelegramBotWorker(TelegramLogicHandlers telegramLogicHandlers, IConfiguration configuration)
        {
            _telegramLogicHandlers = telegramLogicHandlers;
            _configuration = configuration;
            _botToken = configuration.GetValue<string>("BotToken");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var client = new TelegramBotClient(_botToken);
            if (!stoppingToken.IsCancellationRequested)
            {
                client.StartReceiving(new DefaultUpdateHandler(_telegramLogicHandlers.HandleUpdateAsync, _telegramLogicHandlers.HandleErrorAsync),
                       stoppingToken);
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}
