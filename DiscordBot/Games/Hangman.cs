using Discord.WebSocket;
using DiscordBot.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Games
{
    public class Hangman : Game
    {
        public Hangman(ulong RoomID, DiscordSocketClient client) : base(RoomID, client) {}

        public override async Task MessegeResive(SocketUserMessage message)
        {
            await CheckForCommands(message);
            await message.DeleteAsync();
            //await SendMessege(message.Content);
        }

        protected override async Task Startup()
        {
            await Task.Delay(1 * 1000);
            await SendMessege("Wating for players type \"join\" to join");
        }

        private async Task<bool> CheckForCommands(SocketUserMessage message)
        {
            switch (message.Content.ToLower())
            {
                case "join":
                    Console.WriteLine($"{_RoomID} : {message.Author.Username} Has joined the game");
                    users.Add(message.Author);
                    await SendMessege($"{message.Author.Username} Has joined the game");
                    return true;
                case "leave":
                    Console.WriteLine($"{_RoomID} {message.Author.Username} Has Leaved the game");
                    users.Remove(message.Author);
                    await SendMessege($"{message.Author.Username} Has Leaved the game");
                    return true;
            }

            return false;
        }
    }
}
