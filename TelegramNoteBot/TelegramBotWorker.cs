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
        public TelegramBotWorker(TelegramLogicHandlers telegramLogicHandlers)
        {
            _telegramLogicHandlers = telegramLogicHandlers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                var client = new TelegramBotClient(Program.botToken);
                client.StartReceiving(new DefaultUpdateHandler(_telegramLogicHandlers.HandleUpdateAsync, _telegramLogicHandlers.HandleErrorAsync),
                       stoppingToken);
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}
