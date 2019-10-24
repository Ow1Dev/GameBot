using Discord.WebSocket;
using DiscordBot.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Games
{
    public class Hangman : Game
    {
        private bool _IsDeleting = false;

        public Hangman(ulong RoomID, DiscordSocketClient client, ushort MaxPlayer = 4, ushort MinPlayer = 1) 
            : base(RoomID, client) { _maxUsers = MaxPlayer; _minUsers = MinPlayer; }

        protected override void Startup()
        {
            Task.Delay(1 * 1000).Wait();
            while(_isRunning)
            {
                waitingForPlayers();
                SendMessege("The Game has started");
                Task.Delay(10 * 1000).Wait();
            }
        }

        protected override async Task _MessegeResive(SocketUserMessage message)
        {
            if (message.Author.IsBot) return;

            bool res = await CheckForCommands(message);
            if (_IsDeleting || res) await message.DeleteAsync();
        }

        private void waitingForPlayers()
        {
            _IsDeleting = true;
            SendMessege("Wating for players type \"join\" to join");

            while (users.Count < MinUsers) { Task.Delay(1 * 1000).Wait();}
            SendMessege("The Game begins in 10 secounds");
            Task.Delay(5 * 1000).Wait();
            SendMessege("The Game begins in 5 secounds");
            Task.Delay(2 * 1000).Wait();
            SendMessege("The Game begins in 3 secounds");
            Task.Delay(1 * 1000).Wait();
            SendMessege("The Game begins in 2 secounds");
            Task.Delay(1 * 1000).Wait();
            SendMessege("The Game begins in 1 secounds");
            Task.Delay(1 * 1000).Wait();
            _IsDeleting = false;
        }


        private async Task<bool> CheckForCommands(SocketUserMessage message)
        {
            switch (message.Content.ToLower())
            {
                case "join":
                    await OnJoin(message);
                    return true;
                case "leave":
                    await OnLeave(message);
                    return true;
            }
            return false;
        }

        private async Task OnJoin(SocketUserMessage message)
        {
            if(users.Count >= _maxUsers)
            {
                Console.WriteLine($"{_RoomID} : {message.Author.Username} becurse there are to many");
                await SendMessegeAsync($"{message.Author.Username} can not join becourse the game is full");
                return;
            }
            if (users.Any(x => message.Author.Id == x.Id))
            {
                Console.WriteLine($"{_RoomID} : {message.Author.Username} are already in this room");
                await SendMessegeAsync($"{message.Author.Username} are already in this room");
                return;
            }

            users.Add(message.Author);
            Console.WriteLine($"{_RoomID} : {message.Author.Username} Has joined the game");
            await SendMessegeAsync($"{message.Author.Username} Has joined the game");
        }

        private async Task OnLeave(SocketUserMessage message)
        {
            if(!users.Any(x=>message.Author.Id == x.Id))
            {
                Console.WriteLine($"{_RoomID} : {message.Author.Username} are not in the room");
                await SendMessegeAsync($"{message.Author.Username} can not leave becourse there are not in this game");
                return;
            }

            Console.WriteLine($"{_RoomID} : {message.Author.Username} has left the game");
            users.Remove(message.Author);
            await SendMessegeAsync($"{message.Author.Username} has left the game");
        }
    }
}
