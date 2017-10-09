using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Telegram.Bot;
using TelegramBot.Base;

namespace TelegramBot.Host
{
    internal class ConsoleClient
    {

        private static TelegramBotClient _bot;

        public static List<ITelegramBotHandler> Handlers = new List<ITelegramBotHandler>();

        private static void Main(string[] args)
        {
            var handlersToLoad = System.IO.Directory.EnumerateFiles("Plugins/", "*.dll");

            var assembliesToCall = handlersToLoad.Select(x => System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), x)).ToList();

            Console.WriteLine($"Loaded plugins ({assembliesToCall.Count}): ");
            assembliesToCall.ForEach(Console.WriteLine);

            string key = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(key))
            {
                try
                {

                    key = System.IO.File.ReadAllText("Telegram.key");
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine("No key!");
                Console.ReadKey();
                return;
            }

            _bot = new TelegramBotClient(key);

            Handlers = assembliesToCall.Select(LoadHandlerFromFile).ToList();

            Console.WriteLine("Initializing...");
            Handlers.ForEach(x => x.Init(_bot,Handlers));

            var me = _bot.GetMeAsync().Result;

            Console.WriteLine($"{me.Username} is initialized");

            Console.Title = me.Username;

            _bot.StartReceiving();

            Handlers.ForEach(x => x.Start(_bot));

            Console.WriteLine("Everything up and running. Press any key to stop.");
            Console.ReadKey();

            Handlers.ForEach(x => x.Stop());

            _bot.StopReceiving();

        }

        static ITelegramBotHandler LoadHandlerFromFile(string fileName)
        {
            Assembly asm = Assembly.LoadFrom(fileName);
            Type type = asm.GetTypes().FirstOrDefault(x => typeof(ITelegramBotHandler).IsAssignableFrom(x));
            ITelegramBotHandler plugIn = (ITelegramBotHandler)Activator.CreateInstance(type);
            return plugIn;
        }
    }
}
