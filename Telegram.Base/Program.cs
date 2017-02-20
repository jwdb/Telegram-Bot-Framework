using JWDB.Telegram.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace JWDB.Telegram.Base
{
    class Program
    {

        private static TelegramBotClient Bot = null;

        public static List<IJWDBTelegramHandler> handlers = new List<IJWDBTelegramHandler>();
        static Dictionary<long, List<int>> currentGMs = new Dictionary<long, List<int>>();

        static void Main(string[] args)
        {
            var handlersToLoad = System.IO.Directory.EnumerateFiles("lib/", "*.dll");

            var AssembliesToCall = handlersToLoad.Select(x => System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), x));

            AssembliesToCall.ToList().ForEach(Console.WriteLine);

            string key = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(key))
            {
                try
                {

                    key = System.IO.File.ReadAllText("JWDB.Telegram.key");
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine("No key!");
                Console.ReadKey();
                return;
            }

            Bot = new TelegramBotClient(key);

            handlers = AssembliesToCall.Select(x => LoadHandlerFromFile(x)).ToList();

            Console.WriteLine("Initializing...");
            handlers.ForEach(x => x.Init(Bot));

            var me = Bot.GetMeAsync().Result;

            Bot.OnMessage += Bot_OnMessageAsync;

            Console.WriteLine($"{me.Username} is initialized");

            Console.Title = me.Username;

            Bot.StartReceiving();

            handlers.ForEach(x => x.Start(Bot));

            Console.WriteLine("Everything up and running. Press any key to stop.");
            Console.ReadKey();

            handlers.ForEach(x => x.Stop());

            Bot.StopReceiving();

        }

        static IJWDBTelegramHandler LoadHandlerFromFile(string fileName)
        {
            Assembly asm = Assembly.LoadFrom(fileName);
            Type type = asm.GetTypes().Where(x => typeof(IJWDBTelegramHandler).IsAssignableFrom(x)).FirstOrDefault();
            IJWDBTelegramHandler plugIn = (IJWDBTelegramHandler)Activator.CreateInstance(type);
            return plugIn;
        }

        private static async void Bot_OnMessageAsync(object sender, MessageEventArgs e)
        {
            try
            {
                var message = e.Message;

                string filename = $"GMs_{message.Chat.Id}.txt";

                if (message == null) return;

                if (message.Type == MessageType.TextMessage)
                {
                    if (message.Text.StartsWith("/GMLogin"))
                    {
                        List<string> chatGMS = System.IO.File.ReadAllLines(filename).ToList();

                        if (chatGMS.Contains(message.From.Id.ToString()))
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, $"Access granted for {message.From.Username}");
                            if (currentGMs.ContainsKey(message.Chat.Id))
                            {
                                currentGMs[message.Chat.Id].Add(message.From.Id);
                            }
                            else
                            {
                                currentGMs.Add(message.Chat.Id, new List<int> { message.From.Id });
                            }

                        }
                    }
                    else if (message.Text.StartsWith("/listPlugins") && currentGMs.ContainsKey(message.Chat.Id) && currentGMs[message.Chat.Id].Contains(message.From.Id))
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, $"Plugins: {Environment.NewLine} {string.Join(Environment.NewLine,handlers.Select(x => x.ToString()))}");
                    }
                    else if (message.Text.StartsWith("/stopPlugin") && currentGMs.ContainsKey(message.Chat.Id) && currentGMs[message.Chat.Id].Contains(message.From.Id))
                    {
                        string pluginString = message.Text.Replace("/stopPlugin ", "");

                        handlers.Where(x => x.ToString() == pluginString).ToList().ForEach(x => x.Stop());
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
