using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Util
{
    public static class Debug
    {
        public static void Log(object msg)
        {
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} : " + msg.ToString());
        }
    }
}
