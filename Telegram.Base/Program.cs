using JWDB.Telegram.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Telegram.Base
{
    class Program
    {

        private static TelegramBotClient Bot = null;

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

            var handlers = AssembliesToCall.Select(x => LoadHandlerFromFile(x)).ToList();

            Console.WriteLine("Initializing...");
            handlers.ForEach(x => x.Init(Bot));

            var me = Bot.GetMeAsync().Result;

            Console.WriteLine($"{me.Username} is initialized");

            Console.Title = me.Username;

            Bot.StartReceiving();

            handlers.ForEach(x => x.Init(Bot));

            Console.WriteLine("Everything up and running. Press any key to stop.");
            Console.ReadKey();

            handlers.ForEach(x => x.Init(Bot));

            Bot.StopReceiving();

        }

        static IJWDBTelegramHandler LoadHandlerFromFile(string fileName)
        {
            Assembly asm = Assembly.LoadFrom(fileName);
            Type type = asm.GetTypes().Where(x => typeof(IJWDBTelegramHandler).IsAssignableFrom(x)).FirstOrDefault();
            IJWDBTelegramHandler plugIn = (IJWDBTelegramHandler)Activator.CreateInstance(type);
            return plugIn;
        }
    }
}
