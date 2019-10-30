using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class Ping : InteractiveBase<SocketCommandContext>
    {
        [Command("Say")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task SayAsync(string text)
        {
            Util.Debug.Log($"{Context.Message.Author.Username} : {Context.Message.ToString()}");
            await ReplyAsync($"```{text}```");
        }
    }
}
