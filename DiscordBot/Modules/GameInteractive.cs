using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Data;
using DiscordBot.Games;
using System;
using System.Collections.Generic;
using System.IO;
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
        public async Task Start(string Catergory)
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await ReplyAndDeleteAsync(":x: You are not the gamemaster. " + Context.User.Mention, timeout: new TimeSpan(0, 0, 15));
                return;
            }

            string folderPath = @$"{ Directory.GetCurrentDirectory()}\Catergory\";
            if (!File.Exists(folderPath + Catergory + ".caty"))
            {
                await ReplyAndDeleteAsync($"Catergory does not exist", timeout: new TimeSpan(0, 0, 15));
                return;
            }


            var words = File.ReadAllLines(folderPath + Catergory + ".caty");
            if(words.Length < 1)
            {
                await ReplyAndDeleteAsync($"There are no words in **{Catergory}**", timeout: new TimeSpan(0, 0, 15));
                return;
            }

            var c = Context.Guild.CategoryChannels.SingleOrDefault(x => x.Name == "Games");
            if(c == null)
            {
                Console.WriteLine("There is no Catergory named Games");
                return;
            }

            RestTextChannel Room = await Context.Guild.CreateTextChannelAsync($"Hangman-{Catergory}-{_games.Count + 1}",x => {
                x.CategoryId = c.Id;
            });

            if(Room == null)
            {
                Console.WriteLine("Something went wrong");
                return;
            }

            Hangman game = new Hangman(Room.Id, Context.Client, words);
            _games.Add(game);

            await ReplyAndDeleteAsync($"A Game Has started on <#{Room.Id}>", timeout: new TimeSpan(0, 0, 15));
            game.Start();
        }

        [Command("Upload", RunMode = RunMode.Async)]
        public async Task UploadFile()
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await ReplyAndDeleteAsync(":x: You are not the gamemaster. " + Context.User.Mention, timeout: new TimeSpan(0, 0, 15));
                return;
            }
            var attachments = Context.Message.Attachments;
            if (attachments.Count == 1)
            {
                WebClient myWebClient = new WebClient();
                byte[] buffer = myWebClient.DownloadData(attachments.ElementAt(0).Url);
                 myWebClient.Dispose();

                string filename = attachments.ElementAt(0).Filename;

                int lastindex = filename.LastIndexOf('.') + 1;
                string catergoryName = filename.Substring(0, filename.Length - (filename.Length - filename.LastIndexOf('.')));
                string filepath = @$"{ Directory.GetCurrentDirectory()}\Catergory\{catergoryName}.caty"; 
                
                if(filename.Substring(filename.LastIndexOf('.'), filename.Length - filename.LastIndexOf('.')) != ".txt")
                {
                    await ReplyAndDeleteAsync($"It must be a txt file", timeout: new TimeSpan(0, 0, 15));
                    return;
                }

                string download = Encoding.UTF8.GetString(buffer);
                download = download.Replace("\r\n", "%&");
                var _words = download.Split("%&");

                if(!Directory.Exists($@"{ Directory.GetCurrentDirectory()}\Catergory\"))
                {
                    Directory.CreateDirectory($@"{ Directory.GetCurrentDirectory()}\Catergory\");
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filepath, append: false))
                {
                    foreach (string word in _words)
                    {
                        file.WriteLine(word);
                    }
                }

                await ReplyAndDeleteAsync($"You have upload **{catergoryName}**", timeout: new TimeSpan(0, 0, 15));
            } else
            {
                await ReplyAndDeleteAsync($"You need to attact a file", timeout: new TimeSpan(0, 0, 15));
            }
        }

        [Command("Upload Delete", RunMode = RunMode.Async)]
        public async Task DeleteFile(string name)
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await ReplyAndDeleteAsync(":x: You are not the gamemaster. " + Context.User.Mention, timeout: new TimeSpan(0, 0, 15));
                return;
            }

            string folderPath = @$"{ Directory.GetCurrentDirectory()}\Catergory\";
            if(!File.Exists(folderPath + name + ".caty"))
            {
                await ReplyAndDeleteAsync($"Catergory does not exist", timeout: new TimeSpan(0, 0, 15));
                return;
            }

            File.Delete(folderPath + name + ".caty");
            await ReplyAndDeleteAsync($"**{name}** has been deleted", timeout: new TimeSpan(0, 0, 15));
        }

        [Command("Upload List", RunMode = RunMode.Async)]
        public async Task ListFile()
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await ReplyAndDeleteAsync(":x: You are not the gamemaster. " + Context.User.Mention, timeout: new TimeSpan(0, 0, 15));
                return;
            }

            string result = "";
            string folderPath = @$"{ Directory.GetCurrentDirectory()}\Catergory\";
            var files = Directory.GetFiles(folderPath);
            if(files.Length < 1)
            {
                await ReplyAndDeleteAsync($"No Catergory", timeout: new TimeSpan(0, 0, 15));
                return;                                                                     
            }
            
            for (int i = 0; i < files.Length; i++)
            {
                result += files[i].Substring(files[i].LastIndexOf("\\") + 1, files[i].Length - (files[i].LastIndexOf("\\") + 1) - 5) + "\n" ;
            }
            await ReplyAndDeleteAsync(result, timeout: new TimeSpan(0, 0, 15));
        }

        [Command("Stop")] 
        public async Task Stop(SocketChannel channel)
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await ReplyAndDeleteAsync(":x: You are not the gamemaster. " + Context.User.Mention, timeout: new TimeSpan(0, 0, 15));
                return;
            }

            var game = _games.SingleOrDefault(x => x._RoomID == channel.Id);
            if (game == null)
                return;

            game.Stop();
            await Context.Guild.Channels.SingleOrDefault(x => x.Id == channel.Id).DeleteAsync();
            _games.Remove(game);
            await ReplyAndDeleteAsync($"The Games has stopped", timeout: new TimeSpan(0, 0, 15));
        }

        [Command("Force")]
        public async Task Force(SocketChannel channel)
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await ReplyAndDeleteAsync(":x: You are not the gamemaster. " + Context.User.Mention, timeout: new TimeSpan(0, 0, 15));
                return;
            }

            var game = _games.SingleOrDefault(x => x._RoomID == channel.Id);
            if (game == null)
                return;

            await game.Force();
            await ReplyAndDeleteAsync($"<#{game._RoomID}> has been forced", timeout: new TimeSpan(0, 0, 15));
        }

        [Command("List")]
        public async Task ListPlayers(SocketChannel channel)
        {
            if (!UserIsGameMater((SocketGuildUser)Context.User))
            {
                await ReplyAndDeleteAsync(":x: You are not the gamemaster. " + Context.User.Mention, timeout: new TimeSpan(0, 0, 15));
                return;
            }

            var game = _games.SingleOrDefault(x => x._RoomID == channel.Id);
            if (game == null)
                return;

            if(game.users.Count == 0)
            {
                await ReplyAndDeleteAsync($"No Playes has joined", timeout: new TimeSpan(0, 0, 15));
                return;
            }

            await ReplyAndDeleteAsync(
                $"Players({game.users.Count}/{game.MaxUsers}) \n" +
                "------------" + "\n" +
                String.Join("\n", game.users.Select(x=> x.Username)), timeout: new TimeSpan(0, 0, 15));
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
