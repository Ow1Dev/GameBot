using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Data
{
    public abstract class Game
    {
        public ulong _RoomID { get; set; }
        public List<SocketUser> users { get; set; } = new List<SocketUser>();
        private ushort _maxUsers = ushort.MaxValue;
        private DiscordSocketClient _client;

        public Game(ulong RoomID, DiscordSocketClient client)
        {
            _RoomID = RoomID;
            _client = client;
        }

        public async Task SendMessege(string text)
        {
            SocketGuild guild = _client.Guilds.First();
            SocketTextChannel textc = guild.GetTextChannel(_RoomID);

            await textc.SendMessageAsync(text);
        }

        protected abstract Task Startup();
        public abstract Task MessegeResive(SocketUserMessage message);
        
        public void Start()
        {
            Startup();
        }

        public ushort MaxUsers
        {
            get { return _maxUsers; }
        }

    }
}
