using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.Games
{
    public class Hangman : Data.Game
    {
        private bool _IsDeleting = false;
        private bool _HasGuessed = false;
        private bool _HasBegun = false;

        private string _res = "";

        private ulong SelecedUserID = 0;
        private int _Lifes = 6;
        private string _Word = "Hangman";
        private string[] _Words;

        private List<char> _GuessedLetter = new List<char>();
        private char _lastGuess = '_';

        private List<ulong> _GuessedUser = new List<ulong>();
        private Dictionary<ulong, ushort> timeouts = new Dictionary<ulong, ushort>();

        public Hangman(ulong RoomID, DiscordSocketClient client, string[] Words, ushort MaxPlayer = 4, ushort MinPlayer = 1)
            : base(RoomID, client) { _Words = Words; _maxUsers = MaxPlayer; _minUsers = MinPlayer; }

        protected override void Startup()
        {
            Task.Delay(1 * 1000).Wait();
            while (_isRunning)
            {
                int counter = 0;

                waitingForPlayers();

                string newWord = "";
                do
                {
                    Random r = new Random();
                    newWord = _Words[r.Next(0, _Words.Count())];
                } while (_Word == newWord && _Words.Count() > 1);
                _Word = newWord;

                //Init Game
                _GuessedLetter.Clear();
                _HasBegun = true;

                NextUser();

                _res = CheckContents(_Word, _GuessedLetter);
                PrintGame("The Game Has Started").Wait();
                SendMessege($"<@{SelecedUserID}> turn");
                while (!_IsForce && !_HasGuessed)
                {
                    Task.Delay(1 * 1000).Wait();
                    if (!_IsForce)
                    {

                        var l = _GuessedLetter.Count > 0 ? _GuessedLetter.Last() : '_';
                        if (_lastGuess == l)
                        {
                            counter++;
                        }
                        else
                        {
                            _lastGuess = l;
                            counter = 0;
                        }

                        if (counter > 60)
                        {
                            counter = 0;
                            if (!timeouts.ContainsKey(SelecedUserID))
                            {
                                timeouts.Add(SelecedUserID, 0);
                            }

                            if (timeouts[SelecedUserID] >= 5)
                            {
                                var u = users.SingleOrDefault(x => x.Id == SelecedUserID);
                                if (u != null)
                                {
                                    Leave(u).Wait();
                                    timeouts.Remove(SelecedUserID);
                                }
                            }
                            else
                            {
                                Util.Debug.Log(timeouts[SelecedUserID]);
                                timeouts[SelecedUserID]++;
                                if (users.Count() > 1)
                                {
                                    NextUser();
                                    SendMessege($"<@{SelecedUserID}> turn");
                                }
                            }
                        }
                        else if (counter == 30)
                        {
                            SendMessege($"You will lose the turn in about 30 secounds");
                        }
                        else if (counter == 50)
                        {
                            SendMessege($"You will lose the turn in about 10 secounds");
                        }
                    }
                }
                if (_IsForce)
                {
                    SendMessege("The Game has been forced");
                }

                CleanRoom().Wait();

                timeouts.Clear();
                _HasBegun = false;

                _res = "";
                _lastGuess = '_';
                _Lifes = 6;

                _HasGuessed = false;
                _IsForce = false;
            }
        }

        private async Task CleanRoom()
        {
            var room = getRoom();
            var messages = await room.GetMessagesAsync(100 + 1).FlattenAsync();
            await room.DeleteMessagesAsync(messages);
        }

        protected override async Task _MessegeResive(SocketUserMessage message)
        {
            bool res = await CheckForCommands(message);
            if (_IsDeleting || res || !users.Any(x => x.Id == message.Author.Id))
            {
                await message.DeleteAsync();
                return;
            }

            if (message.Author.Id != SelecedUserID)
            {
                await message.DeleteAsync();
                return;
            }

            if (message.Content.ToLower() == _Word.ToLower())
            {
                _HasGuessed = true;
                await PrintGame($"Your guess the word, the word was **{_Word}**");
                return;
            }

            if (message.Content.Length != 1)
            {
                await message.DeleteAsync();
                return;
            }

            timeouts[message.Author.Id] = 0;
            if (!_HasGuessed)
            {
                if (!Regex.IsMatch(message.Content.ToUpper(), @"[A-Zæøå]+"))
                {
                    await message.DeleteAsync();
                    return;
                }
                GuessLetter(message.Content.ToUpper()[0]);

                NextUser();
                SendMessege($"<@{SelecedUserID}> turn");
            }
        }

        private void GuessLetter(char letter)
        {
            if (_GuessedLetter.Any(x => x == letter))
            {
                PrintGame($"Your All ready guessed {letter}").Wait();
                return;
            }

            _GuessedLetter.Add(letter.ToString().ToUpper()[0]);
            _res = CheckContents(_Word, _GuessedLetter);

            if (_res.ToUpper() == _Word.ToUpper())
            {
                _HasGuessed = true;
                PrintGame($"Your guess the word, the word was **{_Word}**").Wait();
                return;
            }

            if (_Word.ToUpper().Contains(letter))
            {
                PrintGame($"**{letter}** is in the word").Wait();
            }
            else
            {
                _Lifes--;
                if (_Lifes <= 0)
                {
                    _HasGuessed = true;
                    PrintGame($"You Lost the Game. The Word was **{_Word}**").Wait();
                }
                else
                {
                    PrintGame($"**{letter}** is not in the word").Wait();
                }

            }
        }

        private async Task PrintGame(string text)
        {
            var c = string.Join(',', _GuessedLetter.ToArray());
            var p = !_HasGuessed ? PrintNon(_res) : PrintNon(_Word);

            string imgurl = "";
            switch (_Lifes)
            {
                case 1:
                    imgurl = "https://i.imgur.com/qpZWkKj.png";
                    break;
                case 2:
                    imgurl = "https://i.imgur.com/Dv9xx5n.png";
                    break;
                case 3:
                    imgurl = "https://i.imgur.com/5YS0NrP.png";
                    break;
                case 4:
                    imgurl = "https://i.imgur.com/pJEhAsC.png";
                    break;
                case 5:
                    imgurl = "https://i.imgur.com/9nmmQQX.png";
                    break;
                case 6:
                    imgurl = "https://i.imgur.com/VLkXPOD.png";
                    break;
                default:
                    imgurl = "https://i.imgur.com/Hi7YyJy.png";
                    break;

            }

            var e = new EmbedBuilder
            {
                Title = text,
                Description = p,
                ThumbnailUrl = imgurl

            }
            .AddField("Lives: ", _Lifes, inline: true)
            .AddField("Gussed Letters", c != "" ? c : "you have not gussed anything", inline: true)
            .WithColor(_HasGuessed ? Color.Green : new Color(255, 99, 71)).Build();

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
                else if (tempString[i] == ' ')
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
            string result = "";
            char[] wordArr = word.ToUpper().ToArray();
            for (int i = 0; i < wordArr.Length; i++)
            {
                if (!Regex.IsMatch(wordArr[i].ToString(), @"[A-Zæøå]"))
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
            if (users.Count < 2)
            {
                SelecedUserID = users[0].Id;
                return;
            };

            if (_GuessedUser.Count >= users.Count) _GuessedUser.Clear();

            Random r = new Random();
            ulong s = 0;
            do
            {
                s = users[r.Next(MinUsers - 1, users.Count)].Id;

            } while (SelecedUserID == s || _GuessedUser.Any(x => x == s));

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

            for (int GameIndex = 0; GameIndex < Modules.GameInteractive._games.Count; GameIndex++)
            {
                for (int uIndex = 0; uIndex < Modules.GameInteractive._games[GameIndex].users.Count; uIndex++)
                {
                    if(Modules.GameInteractive._games[GameIndex].users[uIndex].Id == message.Author.Id)
                    {
                        await SendMessegeAsync($"{message.Author.Username} can not join becourse his already in game");
                    }
                }
            }

            if (users.Count >= _maxUsers)
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
            await Leave(message.Author);
        }

        public async Task Leave(SocketUser user)
        {
            if (!users.Any(x => user.Id == x.Id))
            {
                Console.WriteLine($"{_RoomID} : {user.Username} are not in the room");
                await SendMessegeAsync($"{user.Username} can not leave becourse there are not in this game");
                return;
            }

            Console.WriteLine($"{_RoomID} : {user.Username} has left the game");
            users.Remove(user);
            await SendMessegeAsync($"{user.Username} has left the game");

            if (users.Count <= 0 && _HasBegun)
            {
                await SendMessegeAsync("The Game Has stopped becourse thete are no more player");
                _HasGuessed = true;
            }
            else if (SelecedUserID == user.Id)
            {
                NextUser();
                await SendMessegeAsync($"<@{SelecedUserID}> turn");
            }
        }

        #endregion
    }
}