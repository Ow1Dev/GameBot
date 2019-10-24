using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class Ping : InteractiveBase<SocketCommandContext>
    {
        [Command("Math", RunMode = RunMode.Async)]
        public async Task PingAsync()
        {
            Random r = new Random();
            int num1 = r.Next(1, 10);
            int num2 = r.Next(1, 10);

            await ReplyAsync($"What is {num1} + {num2}?");

            SocketMessage response = null;
            do
            {
                response = await NextMessageAsync();
                //if(response == null)
                    //await ReplyAsync("Come on this is easy");
            } while (response == null || response.Author.IsBot);
                
            if (!response.Author.IsBot)
            {
                if(int.TryParse(response.ToString(), out int result))
                {
                    if (result == (num1 + num2))
                        await ReplyAsync("You Did it boiiiii");
                    else
                        await ReplyAsync("noooooo");
                } else
                {
                    await ReplyAsync($"Ohhhh look {response.ToString()} is not a number!!");    
                }
                return;
            }
        }

        [Command("Say")]
        public async Task SayAsync(string text)
        {
            Console.WriteLine($"{Context.Message.Author.Username} : {Context.Message.ToString()}");
            await ReplyAsync(text);
        }
    }
}
