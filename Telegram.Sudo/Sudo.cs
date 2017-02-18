using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JWDB.Telegram.Base;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Telegram.Sudo
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

        private static async void Bot_OnMessageAsync(object sender, Bot.Args.MessageEventArgs e)
        {
            try
            {
                var message = e.Message;

                string filename = $"users_{message.Chat.Id}.txt";

                if (message == null) return;

                if (message.Type == MessageType.TextMessage)
                {
                    Console.WriteLine("Got Text Message");
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
                    else if (message.Text.StartsWith("/join"))
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

        public void Start(TelegramBotClient bot)
        {
            
        }

        public void Stop()
        {
            
        }
    }
}
