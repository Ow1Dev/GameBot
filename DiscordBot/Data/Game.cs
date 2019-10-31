using Discord;
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
        protected bool _IsForce = false;
        public ulong _RoomID { get; set; }
        public List<SocketUser> users { get; set; } = new List<SocketUser>();

        private Thread thread;

        private DiscordSocketClient _client;

        protected ushort _maxUsers = ushort.MaxValue;
        protected ushort _minUsers = ushort.MinValue;

        protected bool _isRunning { get; set; } = true;

        public Game(ulong RoomID, DiscordSocketClient client)
        {
            _RoomID = RoomID;
            _client = client;
        }

        public async Task SendMessegeAsync(string text)
        {
            var textc = getRoom();
            await textc.SendMessageAsync(text);
        }

        public SocketTextChannel getRoom()
        {
            SocketGuild guild = _client.Guilds.First();
            return guild.GetTextChannel(_RoomID);

        }

        public void Force()
        {
            _IsForce = true;
        }

        public void SendMessege(string text)
        {
            SocketGuild guild = _client.Guilds.First();
            SocketTextChannel textc = guild.GetTextChannel(_RoomID);

            textc.SendMessageAsync(text).Wait();
        }

        public async Task SendEmbedMessege(Embed embed)
        {
            SocketGuild guild = _client.Guilds.First();
            SocketTextChannel textc = guild.GetTextChannel(_RoomID);

            await textc.SendMessageAsync(embed: embed);
        }

        public async Task MessegeResive(SocketUserMessage message)
        {
            await _MessegeResive(message);
        }

        protected abstract void Startup();
        protected abstract Task _MessegeResive(SocketUserMessage message);
        
        public void Start()
        {
            thread = new Thread(Startup);
            thread.Start();
        }

        public void Stop()
        {
            users.Clear();
            _isRunning = false;
        }

        public ushort MaxUsers
        {
            get { return _maxUsers; }
        }

        public ushort MinUsers
        {
            get { return _minUsers; }
        }

    }
}
