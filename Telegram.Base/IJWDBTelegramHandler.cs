using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace JWDB.Telegram.Base
{
    public interface IJWDBTelegramHandler
    {
        void Init(TelegramBotClient bot);

        void Start(TelegramBotClient bot);

        void Stop();
    }
}
