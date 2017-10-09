using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TelegramBot.Base;

namespace TelegramBot.Hitman
{
    public class HitmanOrderHandler : ITelegramBotHandler
    {
        static readonly Random Rnd = new Random();

        static Dictionary<long, int> _currentTargets = new Dictionary<long, int>();

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
            _botClient.OnMessage -= Bot_OnMessageAsync;
            _currentTargets.ToList().ForEach(x => _botClient.SendTextMessageAsync(x.Key, $"The hitman went away..."));
            _currentTargets = new Dictionary<long, int>();
        }

        private static async void Bot_OnMessageAsync(object sender, MessageEventArgs e)
        {
            try
            {
                var message = e.Message;

                if (message == null) return;

                if (message.Type == MessageType.TextMessage)
                {
                    if (_currentTargets.ContainsKey(message.Chat.Id) && message.Text.StartsWith("/call911") && message.From.Id == _currentTargets[message.Chat.Id])
                    {
                        if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
                        {
                            _currentTargets.Remove(message.Chat.Id);
                            await _botClient.SendTextMessageAsync(message.Chat.Id, "911 has been called, the hitman will search for a new target...");

                            await DetermineNewTarget(message.Chat.Id);
                        }
                    }
                    else if (!_currentTargets.ContainsKey(message.Chat.Id) && message.Text.StartsWith("/orderAHitman"))
                    {
                        if (message.Chat.Type == ChatType.Group || message.Chat.Type == ChatType.Supergroup)
                        {
                            await _botClient.SendTextMessageAsync(message.Chat.Id, "911 has been called, the hitman will search for a new target...");

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

                int targetWas = _currentTargets[chatId];

                var getnameTask = _botClient.GetChatMemberAsync(chatId, targetWas);

                getnameTask.Start();

                getnameTask.Wait();

                var name = string.IsNullOrWhiteSpace(getnameTask.Result.User.Username) ? getnameTask.Result.User.FirstName : getnameTask.Result.User.Username;

                var filename = "deathmessages.txt";

                List<string> dmesgs = System.IO.File.ReadAllLines(filename).Distinct().ToList();

                int r = Rnd.Next(dmesgs.Count);

                var message = dmesgs[r];

                if (message.Contains("{0}"))
                    message = string.Format("{0}", name);

                await _botClient.SendTextMessageAsync(chatId, message);

                _currentTargets.Remove(chatId);

                await DetermineNewTarget(chatId, targetWas);

            }
            catch
            {
            }
        }

        private static async Task DetermineNewTarget(long chatId, int excludeId = 0)
        {

            string filename = $"users_{chatId}.txt";

            List<string> currentUsers = System.IO.File.ReadAllLines(filename).Distinct().ToList();

            int r = Rnd.Next(currentUsers.Count);

            while (excludeId != 0 && excludeId == r)
                r = Rnd.Next(currentUsers.Count);

            var user = currentUsers[r] ?? throw new ArgumentNullException("currentUsers[r]");

            await _botClient.GetChatMemberAsync(chatId, Convert.ToInt32(user)).ContinueWith(x =>
            {
                var targetName = string.IsNullOrWhiteSpace(x.Result.User.Username) ? x.Result.User.FirstName : x.Result.User.Username;

                _currentTargets.Add(chatId, x.Result.User.Id);
                _botClient.SendTextMessageAsync(chatId, $"The target is: @{targetName} call 911 before he kills you!");

                Task.Delay(new TimeSpan(1, 0, 0)).ContinueWith(y => OuttaTime(chatId));
            });
        }
    }
}
