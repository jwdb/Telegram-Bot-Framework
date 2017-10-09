using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TelegramBot.Base;

namespace TelegramBot.Join
{
    public class JoinHandler : ITelegramBotHandler
    {
        static TelegramBotClient _botClient;

        public void Init(TelegramBotClient bot, List<ITelegramBotHandler> handlers)
        {
            _botClient = bot;
            bot.OnMessage += Bot_OnMessageAsync;
        }

        public void Start(TelegramBotClient bot)
        {
            bot.OnMessage += Bot_OnMessageAsync;
        }

        public void Stop()
        {
            _botClient.OnMessage += Bot_OnMessageAsync;
        }

        public static void Bot_OnMessageAsync(object sender, MessageEventArgs e)
        {
            try
            {
                var message = e.Message;

                string filename = $"users_{message.Chat.Id}.txt";

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
