using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JWDB.Telegram.Base;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Args;

namespace JWDB.Telegram.Sudo
{
    public class Sudo : IJWDBTelegramHandler
    {
        static Random rnd = new Random();

        static TelegramBotClient botClient = null;
        
        public void Init(TelegramBotClient bot)
        {
            botClient = bot;
            bot.OnMessage += Bot_OnMessageAsync;
         
        }

        private static async void Bot_OnMessageAsync(object sender, MessageEventArgs e)
        {
            try
            {
                var message = e.Message;

                string filename = $"users_{message.Chat.Id}.txt";

                if (message == null) return;

                if (message.Type == MessageType.TextMessage)
                {
                    if (message.Text.StartsWith("/sudo"))
                    {
                        if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
                        {
                            List<string> currentUsers = System.IO.File.ReadAllLines(filename).Distinct().ToList();

                            int r = rnd.Next(currentUsers.Count);

                            var user = currentUsers[r];

                            await botClient.GetChatMemberAsync(e.Message.Chat.Id, Convert.ToInt32(currentUsers[r])).ContinueWith(x => {
                                var nameToSudo = string.IsNullOrWhiteSpace(x.Result.User.Username) ? x.Result.User.FirstName : x.Result.User.Username;

                                var messageToSudo = message.Text.Replace("/sudo", "").Trim();

                                botClient.SendTextMessageAsync(e.Message.Chat.Id, $"sudo @{nameToSudo} {messageToSudo}");
                                });
                        }
                    }
                }
               

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Start(TelegramBotClient bot)
        {
            
        }

        public void Stop()
        {
            
        }
    }
}
