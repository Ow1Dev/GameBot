using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Data;
using DiscordBot.Games;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    [Group("Game")]
    public class GameInteractive : InteractiveBase<SocketCommandContext>
    {
        public static List<Hangman> _games = new List<Hangman>();

        [Command("Start")]
        public async Task Start()
        {
            var Category = Context.Guild.CategoryChannels.SingleOrDefault(x => x.Name == "Games");
            if(Category == null)
            {
                Console.WriteLine("There is no Catergory named Games");
                return;
            }

            RestTextChannel Room = await Context.Guild.CreateTextChannelAsync($"Game-{_games.Count + 1}",x => {
                x.CategoryId = Category.Id;
            });

            if(Room == null)
            {
                Console.WriteLine("Something went wrong");
                return;
            }

            Hangman game = new Hangman(Room.Id, Context.Client);
            _games.Add(game);

            await ReplyAsync($"A Game Has started on <#{Room.Id}>");
            game.Start();
        }

        [Command("Stop")]
        public async Task Stop(SocketChannel channel)
        {
            var game = _games.SingleOrDefault(x => x._RoomID == channel.Id);
            if (game == null)
                return;

            game.Stop();
            await Context.Guild.Channels.SingleOrDefault(x => x.Id == channel.Id).DeleteAsync();
            await ReplyAsync($"The Games has stopped");
        }

        [Command("Force")]
        public async Task Force(SocketChannel channel)
        {
            var game = _games.SingleOrDefault(x => x._RoomID == channel.Id);
            if (game == null)
                return;

            await game.Force();
            await ReplyAsync($"<#{game._RoomID}> has been forced");
        }


        //[Command("Join")]
        //public async Task Join()
        //{
        //    var game = _games.SingleOrDefault(x => x._RoomID == Context.Channel.Id);
        //    if (game == null)
        //        return;

        //    var user = Context.Message.Author;
        //    if (user.IsBot)
        //        return;

        //    if(game.users.Count >= limentPlayer)
        //    {
        //        await ReplyAsync($"{user.Username} Can not joined because there are too many");
        //        return;
        //    }

        //    if(game.users.Any(x=> x.Id == user.Id))
        //    {
        //        await ReplyAsync($"{user.Username} has already joined");
        //        return;
        //    }

        //    game.users.Add(user);
        //    await ReplyAsync($"{user.Username} has just joined the game");
        //    Console.WriteLine($"{user.Username} has just joined the game");
        //}

        [Command("List")]
        public async Task ListPlayers(SocketChannel channel)
        {
            var game = _games.SingleOrDefault(x => x._RoomID == channel.Id);
            if (game == null)
                return;

            if(game.users.Count == 0)
            {
                await ReplyAsync($"No Playes has joined");
                return;
            }

            await ReplyAsync(
                $"Players({game.users.Count}/{game.MaxUsers}) \n" +
                "------------" + "\n" +
                String.Join("\n", game.users.Select(x=> x.Username)));
        }
    }
}
