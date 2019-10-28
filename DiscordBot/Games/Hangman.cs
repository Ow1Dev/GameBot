using Discord;
using Discord.WebSocket;
using DiscordBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Games
{
    public class Hangman : Data.Game
    {
        private bool _IsDeleting = false;
        private bool _HasGuessed = false;
        private bool _HasBegun = false;

        private int _Lifes = 5;
        private string _res = "";

        private ulong SelecedUserID = 0;
        private string _Word = "Hangman";
        private string[] _Words;

        private List<char> _GuessedLetter = new List<char>();
        private List<ulong> _GuessedUser = new List<ulong>();

        public Hangman(ulong RoomID, DiscordSocketClient client, string[] Words, ushort MaxPlayer = 4, ushort MinPlayer = 1) 
            : base(RoomID, client) { _Words = Words; _maxUsers = MaxPlayer; _minUsers = MinPlayer; }

        protected override void Startup()
        {
            Task.Delay(1 * 1000).Wait();
            while(_isRunning)
            {
                waitingForPlayers();

                string newWord = "";
                do
                {
                    Random r = new Random();
                    newWord = _Words[r.Next(0, _Words.Count())];
                } while (_Word == newWord);
                _Word = newWord;

                //Init Game
                _GuessedLetter.Clear();
                _HasBegun = true;

                NextUser();

                _res = CheckContents(_Word, _GuessedLetter);
                PrintGame("The Game Has Started").Wait();
                SendMessege($"<@{SelecedUserID}> turn");
                while(!_IsForce && !_HasGuessed)
                {
                    Task.Delay(1 * 1000).Wait();
                }
                if(_IsForce)
                {
                    SendMessege("The Game has been forced");
                }

                _HasBegun = false;

                _res = "";
                _Lifes = 5;

                _HasGuessed = false;
                _IsForce = false;
            }
        }

        protected override async Task _MessegeResive(SocketUserMessage message)
        {
            bool res = await CheckForCommands(message);
            if (_IsDeleting || res || !users.Any(x => x.Id == message.Author.Id))
            {
                await message.DeleteAsync();
                return;
            }

            if(message.Author.Id != SelecedUserID)
            {
                await message.DeleteAsync();
                return;
            }

            if (message.Content.ToLower() == _Word.ToLower())
            {
                await PrintGame($"Your won the word was **{_Word}**");
                _HasGuessed = true;
                return;
            }

            if (message.Content.Length != 1)
            {
                await message.DeleteAsync();
                return;
            }
            if(!_HasGuessed)
            {
                GuessLetter(message.Content.ToUpper()[0]);

                NextUser();
                SendMessege($"<@{SelecedUserID}> turn");
            }
        }

        private void GuessLetter(char letter)
        {
            if(_GuessedLetter.Any(x => x == letter))
            {
                PrintGame($"Your All ready guessed {letter}").Wait();
                return;
            }

            _GuessedLetter.Add(letter.ToString().ToUpper()[0]);
            _res = CheckContents(_Word, _GuessedLetter);
            
            if(_res == _Word)
            {
                PrintGame($"Your won the word was **{_Word}**").Wait();
                _HasGuessed = true;
                return;
            }
            
            if(_Word.ToUpper().Contains(letter))
            {
                PrintGame($"{letter} is in the word").Wait();
            } else
            {
                _Lifes--;
                if(_Lifes <= 0)
                {
                    PrintGame($"You Lost the Game. The Word was **{_Word}**").Wait();
                    _HasGuessed = true;
                } else
                {
                    PrintGame($"{letter} is not in the word").Wait();
                }

            }
        }

        private async Task PrintGame(string text)
        {
            var c = string.Join(',', _GuessedLetter.ToArray());
            var p = PrintNon(_res);

            var e = new EmbedBuilder
            {
                Title = text,
                Description = p
            }
            .AddField("Lives: ", _Lifes, inline: true)
            .AddField("Gussed Letters", c != "" ? c : "you have not gussed anything", inline: true)
            .WithColor(Color.Blue).Build();

            await SendEmbedMessege(e);
        }

        private string PrintNon(string result)
        {
            string Sout = "";
            string tempString = result;
            for (int i = 0; i < result.Length; i++)
            {
                if (tempString[i] == '*')
                {
                    Sout += "*";
                }
                else if(tempString[i] == ' ')
                {
                    Sout += "\u200b \u200b";
                }
                else
                {
                    Sout += tempString[i];
                }
                Sout += " ";
            }

            return Sout;
        }

        private string CheckContents(string word, List<Char> guesses)
        {
            char[] specials = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '!', '-', '_', '?', ';', ':', '<', '>', ' ', '\'' };
            char[] wordArr = word.ToUpper().ToCharArray();
            string result = "";
            for (int i = 0; i < wordArr.Length; i++)
            {
                if (specials.Contains(wordArr[i]))
                {
                    result += word[i];
                }
                else if (!guesses.Contains(wordArr[i]))
                {
                    result += "*";
                }
                else
                {
                    result += word[i];
                }
            }

            return result;
        }

        public void NextUser()
        {
            if (users.Count < 2) {
                SelecedUserID = users[0].Id;
                return;
            };

            if (_GuessedUser.Count >= users.Count) _GuessedUser.Clear();

            Random r = new Random();
            ulong s = 0;
            do
            {
                s = users[r.Next(MinUsers - 1, users.Count)].Id;

            } while(SelecedUserID == s || _GuessedUser.Any(x=>x == s));

            SelecedUserID = s;
            _GuessedUser.Add(s);
        }

        #region PlayersJoinLeave

        private void waitingForPlayers()
        {
            _IsDeleting = true;
            do
            {
                SendMessege("Wating for players type \"join\" to join");
                while (users.Count < MinUsers) { Task.Delay(1 * 1000).Wait(); }
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
            } while (users.Count < MinUsers);
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
            if (_HasBegun)
                return;

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

            if(users.Count <= 0 && _HasBegun)
            {
                await SendMessegeAsync("The Game Has stopped becourse thete are no more player");
                _IsForce = true;
            } else if(SelecedUserID == message.Author.Id)
            {
                NextUser();
                await SendMessegeAsync($"<@{SelecedUserID}> turn");
            }
        }

        #endregion
    }
}