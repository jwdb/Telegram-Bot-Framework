using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TelegramBot.Base;

namespace TelegramBot.Sudo
{
    public class Sudo : ITelegramBotHandler
    {
        private static readonly Random Rnd = new Random();

        static TelegramBotClient _botClient;
        
        public void Init(TelegramBotClient bot, List<ITelegramBotHandler> handlers)
        {
            _botClient = bot;
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

                            int r = Rnd.Next(currentUsers.Count);

                            var user = currentUsers[r] ?? throw new ArgumentNullException("currentUsers[r]");

                            await _botClient.GetChatMemberAsync(e.Message.Chat.Id, Convert.ToInt32(currentUsers[r])).ContinueWith(x => {
                                var nameToSudo = string.IsNullOrWhiteSpace(x.Result.User.Username) ? x.Result.User.FirstName : x.Result.User.Username;

                                var messageToSudo = message.Text.Replace("/sudo", "").Trim();

                                _botClient.SendTextMessageAsync(e.Message.Chat.Id, $"sudo @{nameToSudo} {messageToSudo}");
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
