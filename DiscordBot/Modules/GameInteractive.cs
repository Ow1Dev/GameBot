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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    [Group("Game")]
    public class GameInteractive : InteractiveBase<SocketCommandContext>
    {
        public static List<Data.Game> _games = new List<Data.Game>();

        [Command("Start")]
        public async Task Start()
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await Context.Channel.SendMessageAsync(":x: You are not the gamemaster. " + Context.User.Mention);
                return;
            }

            var Category = Context.Guild.CategoryChannels.SingleOrDefault(x => x.Name == "Games");
            if(Category == null)
            {
                Console.WriteLine("There is no Catergory named Games");
                return;
            }

            string[] _words = new string[] { "Hang Man!" };
            var attachments = Context.Message.Attachments;
            if (attachments.Count == 1)
            {
                WebClient myWebClient = new WebClient();

                byte[] buffer = myWebClient.DownloadData(attachments.ElementAt(0).Url);

                string download = Encoding.UTF8.GetString(buffer);

                download = download.Replace("\r\n", "%&");
                _words = download.Split("%&");

                myWebClient.Dispose();
            }

            RestTextChannel Room = await Context.Guild.CreateTextChannelAsync($"Game-{_games.Count + 1}",x => {
                x.CategoryId = Category.Id;
            });

            if(Room == null)
            {
                Console.WriteLine("Something went wrong");
                return;
            }

            Hangman game = new Hangman(Room.Id, Context.Client, _words);
            _games.Add(game);

            await ReplyAndDeleteAsync($"A Game Has started on <#{Room.Id}>", timeout: new TimeSpan(0, 0, 15));
            game.Start();
        }

        [Command("Stop")]
        public async Task Stop(SocketChannel channel)
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await Context.Channel.SendMessageAsync(":x: You are not the gamemaster. " + Context.User.Mention);
                return;
            }

            var game = _games.SingleOrDefault(x => x._RoomID == channel.Id);
            if (game == null)
                return;

            game.Stop();
            await Context.Guild.Channels.SingleOrDefault(x => x.Id == channel.Id).DeleteAsync();
            _games.Remove(game);
            await ReplyAsync($"The Games has stopped");
        }

        [Command("Force")]
        public async Task Force(SocketChannel channel)
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await Context.Channel.SendMessageAsync(":x: You are not the gamemaster. " + Context.User.Mention);
                return;
            }

            var game = _games.SingleOrDefault(x => x._RoomID == channel.Id);
            if (game == null)
                return;

            await game.Force();
            await ReplyAsync($"<#{game._RoomID}> has been forced");
        }

        [Command("List")]
        public async Task ListPlayers(SocketChannel channel)
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await Context.Channel.SendMessageAsync(":x: You are not the gamemaster. " + Context.User.Mention);
                return;
            }

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

        private bool UserIsGameMater(SocketGuildUser user)
        {
            string targetRoleName = "GameMaster";
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }

    }
}
