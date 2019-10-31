using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Data;
using DiscordBot.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        static void Main()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Delete All Games");
        }

        private DiscordSocketClient _client;
        public static CommandService _commands;
        public IServiceProvider _services;

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton<InteractiveService>()
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string botToken = APIKey.BOT_TOKEN;

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(Discord.TokenType.Bot, botToken);
            await _client.StartAsync();
            await _client.SetGameAsync("Hangman");
            

            await Task.Delay(-1);
        }

        private Task _client_Log(Discord.LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += _client_MessageReceived;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message is null || message.Author.IsBot) return;

            int argPos = 0;

            Game game = GameInteractive._games.SingleOrDefault(x => x._RoomID == message.Channel.Id);
            if(game != null)
            {
                    game.MessegeResive(message);
                return;
            }

            if(!message.Channel.Name.Equals("gamecontrol"))
            {
                return;
            }

            if(message.HasStringPrefix("&", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                //Console.WriteLine(message.ToString() + $": {argPos}");
                var context = new SocketCommandContext(_client, message);
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                    Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
