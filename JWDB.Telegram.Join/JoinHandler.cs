using JWDB.Telegram.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace JWDB.Telegram.Join
{
    public class JoinHandler : IJWDBTelegramHandler
    {
        static TelegramBotClient botClient = null;

        public void Init(global::Telegram.Bot.TelegramBotClient bot)
        {
            botClient = bot;
            bot.OnMessage += Bot_OnMessageAsync;
        }

        public void Start(global::Telegram.Bot.TelegramBotClient bot)
        {
        }

        public void Stop()
        {
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
                    Console.WriteLine("Got Text Message");
                    if (message.Text.StartsWith("/join"))
                    {
                        System.IO.File.AppendAllText(filename, message.From.Id + Environment.NewLine);

                    }
                }
                else if (message.Type == MessageType.ServiceMessage)
                {
                    if (message.NewChatMember != null)
                    {
                        System.IO.File.AppendAllText(filename, message.NewChatMember.Id + Environment.NewLine);
                    }
                    else if (message.LeftChatMember != null)
                    {

                        List<string> usersLeft = System.IO.File.ReadAllLines(filename).ToList();
                        usersLeft.Remove(message.LeftChatMember.Id.ToString());
                        System.IO.File.Delete(filename);
                        System.IO.File.WriteAllLines(filename, usersLeft.ToArray());

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
