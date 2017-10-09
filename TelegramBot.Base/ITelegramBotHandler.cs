using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramBot.Base
{
    public interface ITelegramBotHandler
    {
        void Init(TelegramBotClient bot, List<ITelegramBotHandler> handlers);

        void Start(TelegramBotClient bot);

        void Stop();
    }
}
