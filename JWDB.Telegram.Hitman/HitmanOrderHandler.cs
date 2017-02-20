using JWDB.Telegram.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace JWDB.Telegram.Hitman
{
    public class HitmanOrderHandler : IJWDBTelegramHandler
    {
        static Random rnd = new Random();

        static Dictionary<long, int> currentTargets = new Dictionary<long, int>();

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
            currentTargets.ToList().ForEach(x => botClient.SendTextMessageAsync(x.Key, $"The hitman went away..."));
            currentTargets = new Dictionary<long, int>();
        }

        private static async void Bot_OnMessageAsync(object sender, MessageEventArgs e)
        {
            try
            {
                var message = e.Message;

                if (message == null) return;

                if (message.Type == MessageType.TextMessage)
                {
                    if (currentTargets.ContainsKey(message.Chat.Id) && message.Text.StartsWith("/call911") && message.From.Id == currentTargets[message.Chat.Id])
                    {
                        if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
                        {
                            currentTargets.Remove(message.Chat.Id);
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"911 has been called, the hitman will search for a new target...");

                            await DetermineNewTarget(message.Chat.Id);
                        }
                    }
                    else if (!currentTargets.ContainsKey(message.Chat.Id) && message.Text.StartsWith("/orderAHitman"))
                    {
                        if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"911 has been called, the hitman will search for a new target...");

                            await DetermineNewTarget(message.Chat.Id, message.From.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async void OuttaTime(long chatId)
        {
            try
            {

                int targetWas = currentTargets[chatId];

                var getnameTask = botClient.GetChatMemberAsync(chatId, targetWas);

                getnameTask.Start();

                getnameTask.Wait();

                var name = string.IsNullOrWhiteSpace(getnameTask.Result.User.Username) ? getnameTask.Result.User.FirstName : getnameTask.Result.User.Username;

                string filename = $"deathmessages.txt";

                List<string> dmesgs = System.IO.File.ReadAllLines(filename).Distinct().ToList();

                int r = rnd.Next(dmesgs.Count);

                var message = dmesgs[r];

                if (message.Contains("{0}"))
                    message = string.Format("{0}", name);

                await botClient.SendTextMessageAsync(chatId, message);

                currentTargets.Remove(chatId);

                await DetermineNewTarget(chatId, targetWas);

            }
            catch (Exception)
            {
            }
        }

        private static async Task DetermineNewTarget(long chatId, int excludeId = 0)
        {

            string filename = $"users_{chatId}.txt";

            List<string> currentUsers = System.IO.File.ReadAllLines(filename).Distinct().ToList();

            int r = rnd.Next(currentUsers.Count);

            while (excludeId != 0 && excludeId == r)
                r = rnd.Next(currentUsers.Count);

            var user = currentUsers[r];

            await botClient.GetChatMemberAsync(chatId, Convert.ToInt32(currentUsers[r])).ContinueWith(x =>
            {
                var targetName = string.IsNullOrWhiteSpace(x.Result.User.Username) ? x.Result.User.FirstName : x.Result.User.Username;

                currentTargets.Add(chatId, x.Result.User.Id);
                botClient.SendTextMessageAsync(chatId, $"The target is: @{targetName} call 911 before he kills you!");

                Task.Delay(new TimeSpan(1, 0, 0)).ContinueWith(y => OuttaTime(chatId));
            });
        }
    }
}
