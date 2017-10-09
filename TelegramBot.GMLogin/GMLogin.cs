using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TelegramBot.Base;

namespace TelegramBot.GMLogin
{
    public class GMLogin : ITelegramBotHandler
    {

        static readonly Dictionary<long, List<int>> CurrentGMs = new Dictionary<long, List<int>>();
        
        static TelegramBotClient _botClient;

        private static List<ITelegramBotHandler> _handlers;

        public void Init(TelegramBotClient bot, List<ITelegramBotHandler> handlers)
        {
            _botClient = bot;
            _handlers = handlers;

            Start(_botClient);
        }



        public void Start(TelegramBotClient bot)
        {
            _botClient.OnMessage += Bot_OnMessageAsync;

        }

        public void Stop()
        {
            _botClient.OnMessage -= Bot_OnMessageAsync;
        }

        private static async void Bot_OnMessageAsync(object sender, MessageEventArgs e)
        {
            try
            {
                var message = e.Message;

                string filename = $"GMs_{message.Chat.Id}.txt";

                if (message.Type == MessageType.TextMessage)
                {
                    List<string> chatGms = System.IO.File.ReadAllLines(filename).ToList();
                    if (message.Text.StartsWith("/GMLogin"))
                    {
                        if (chatGms.Contains(message.From.Id.ToString()))
                        {
                            await _botClient.SendTextMessageAsync(message.Chat.Id, $"Access granted for {message.From.Username}");
                            if (CurrentGMs.ContainsKey(message.Chat.Id))
                            {
                                CurrentGMs[message.Chat.Id].Add(message.From.Id);
                            }
                            else
                            {
                                CurrentGMs.Add(message.Chat.Id, new List<int> { message.From.Id });
                            }

                        }
                    }
                    else if (message.Text.StartsWith("/listPlugins") && CurrentGMs.ContainsKey(message.Chat.Id) && CurrentGMs[message.Chat.Id].Contains(message.From.Id))
                    {
                        await _botClient.SendTextMessageAsync(message.Chat.Id, $"Plugins: {Environment.NewLine} {string.Join(Environment.NewLine, _handlers.Select(x => x.ToString()))}");
                    }
                    else if (message.Text.StartsWith("/stopPlugin") && CurrentGMs.ContainsKey(message.Chat.Id) && CurrentGMs[message.Chat.Id].Contains(message.From.Id))
                    {
                        string pluginString = message.Text.Replace("/stopPlugin ", "");

                        _handlers.Where(x => x.ToString() == pluginString).ToList().ForEach(x => x.Stop());
                    }
                    else if (message.Text.StartsWith("/startPlugin") && CurrentGMs.ContainsKey(message.Chat.Id) && CurrentGMs[message.Chat.Id].Contains(message.From.Id))
                    {
                        string pluginString = message.Text.Replace("/startPlugin ", "");

                        _handlers.Where(x => x.ToString() == pluginString).ToList().ForEach(x => x.Start(_botClient));
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
