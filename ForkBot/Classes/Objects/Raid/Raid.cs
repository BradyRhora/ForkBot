using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.IO;

namespace ForkBot
{
    public partial class Raid
    {
        static Random rdm = new Random();

        public static bool ChannelHasRaid(IMessageChannel channel)
        {
            return Games.Where(x => x.GetChannel().Id == channel.Id).Count() > 0;
        }
        public static Game GetChannelRaid(IMessageChannel channel)
        {
            return Games.Where(x => x.GetChannel().Id == channel.Id).FirstOrDefault();
        }

        
        
        public static List<Game> Games = new List<Game>();
        public class Game
        {
            public Profile Host { get; }
            List<Player> Players = new List<Player>();
            IMessageChannel Channel;
            public bool Started { get; private set; }
            Room[] Dungeon;
            int currentRoom = 0;
            public static int DUNGEON_SIZE = 10;
            public Game(Profile host, IMessageChannel chan)
            {
                Host = host;
                Join(host);
                Channel = chan;
                Started = false;
                Dungeon = new Room[DUNGEON_SIZE];
            }

            public void Join(Profile user)
            {
                Players.Add(new Player(user, this));
            }

            public IMessageChannel GetChannel()
            {
                return Channel;
            }

            public Player GetPlayer(Profile profile)
            {
                foreach(Player p in GetPlayers())
                {
                    if (p.ID == profile.ID) return p;
                }
                return null;
            }
            public Player[] GetPlayers()
            {
                return Players.ToArray();
            }

            public void Kick(Profile user)
            {
                Players.Remove(user as Player);
            }

            public async Task Start()
            {
                Started = true;
                await Channel.SendMessageAsync("You journey into the mysterious dungeon of Efrüg, knowing not what awaits you and your party...");
                for (int i = 0; i < DUNGEON_SIZE; i++) Dungeon[i] = new Room(i+1, this);
                string action = StateCurrentAction();
                await Channel.SendMessageAsync(action);
            }

            public Room GetCurrentRoom()
            {
                return Dungeon[currentRoom];
            }
            public string ShowCurrentRoom(bool describe = true)
            {
                string msg = "";
                var room = GetCurrentRoom();
                msg += room.BuildBoard();
                if (describe) msg += '\n'+room.DescribeRoom();
                return msg;
            }

            public Placeable GetCurrentTurn()
            {
                var room = GetCurrentRoom();
                var turn = room.Initiative.ElementAt(room.Counter).Key;
                return turn;
            }

            public string StateCurrentAction()
            {
                var room = GetCurrentRoom();
                var turn = GetCurrentTurn();

                string msg = "";
                if (turn.GetType() == typeof(Player))
                {
                    var player = (Player)turn;
                    if (!turn.Moved) msg += room.BuildBoard() + '\n';
                    msg += $"It's {player.GetName()}'s turn.\nChoose one of the folling actions with `;r act [action]`. Don't forget to specify a direction if necessary.\nYou can move {player.StepsLeft} more spaces.\n```\n";
                    var actions = player.GetActions();
                    for (int i = 0; i < actions.Count(); i++)
                    {
                        msg += $"{actions[i].Name}: {actions[i].Description}\n";
                    }
                    msg += "```";
                }
                else if (turn.GetType() == typeof(Monster))
                {
                    var monster = (Monster)turn;
                    msg = monster.ChooseAction(room);
                    room.NextInitiative();
                }

                
                if (turn.GetType() == typeof(Monster)) msg += '\n' + StateCurrentAction();
                return msg;
            }
        }

        public class Room
        {
            Monster[] Enemies;
            int Number;
            Item[] Loot;
            int Size;
            public string[,] Board { get; private set; }
            Game Game;
            public Player[] Players { get; private set; }
            public IOrderedEnumerable<KeyValuePair<Placeable, int>> Initiative;
            public int Counter { get; private set; } = 0;

            public Room(int num, Game game)
            {
                Number = num;
                Game = game;
                Players = Game.GetPlayers();
                GenerateRoom();
                GenerateEnemies();
                PlacePlayers();
                RollInitiative();
            }

            void GenerateRoom()
            {
                Size = rdm.Next(5, 10 + Number / 2);
                if (Size > 16) Size = 16;
                Board = new string[Size, Size];
                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        Board[x, y] = "empty";
                    }
                }
            }

            void GenerateEnemies()
            {
                int enemyLVL = Convert.ToInt32((Decimal.Divide(Number, Game.DUNGEON_SIZE)) * 10);
                int enemyLimit = Players.Count() * 3;
                int enemyCount = Size / 2;
                if (enemyCount > enemyLimit) enemyCount = enemyLimit;

                var possibleEnemies = Monster.Monsters.Where(x => x.Level <= enemyLVL).ToArray();
                Enemies = new Monster[enemyCount];
                for (int i = 0; i < enemyCount; i++)
                {
                    Enemies[i] = possibleEnemies[rdm.Next(possibleEnemies.Count())].Clone();

                    int posX = -1, posY = -1;
                    do
                    {
                        posY = rdm.Next(Size);
                        posX = rdm.Next(Size / 2, Size);
                        Enemies[i].SetLocation(posX, posY);
                    } while (Board[posX, posY] != "empty");
                }

            }

            void PlacePlayers()
            {
                int playerCount = Players.Count();
                int y = Size / 2 - playerCount / 2;
                for (int i = 0; i < playerCount; i++)
                    Players[i].SetLocation(0, y + i);

                if (Players[0].Y == Size)
                {
                    Console.Write("HUH");
                }
            }

            void RollInitiative()
            {
                var rolls = new Dictionary<Placeable, int>();
                foreach (Player p in Players)
                {
                    p.StepsLeft = p.GetMoveDistance();
                    int roll = rdm.Next(10) + 1 + p.GetClass().Speed;
                    rolls.Add(p, roll);
                }

                foreach (Monster m in Enemies)
                {
                    int roll = rdm.Next(10) + 1 + m.Level;
                    rolls.Add(m, roll);
                }

                Initiative = rolls.OrderByDescending(x => x.Value);
            }
            public void NextInitiative()
            {
                Counter++;
                if (Counter >= Initiative.Count()) Counter = 0;
                var turn = Game.GetCurrentTurn();
                turn.StepsLeft = turn.GetMoveDistance();
            }

            public string BuildBoard()
            {
                var b2 = (string[,])Board.Clone();
                for (int i = 0; i < Enemies.Count(); i++) b2[Enemies[i].Y, Enemies[i].X] = "enemy|" + i;
                for (int i = 0; i < Players.Count(); i++) b2[Players[i].Y, Players[i].X] = "player|" + i;


                string board = "";
                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        if (b2[x, y] == "empty") board += "⬛";
                        else if (b2[x, y].StartsWith("enemy|"))
                        {
                            int index = Convert.ToInt32(b2[x, y].Split('|')[1]);
                            board += Enemies[index].GetEmote();
                        }
                        else if (b2[x, y].StartsWith("player|"))
                        {
                            int index = Convert.ToInt32(b2[x, y].Split('|')[1]);
                            board += Game.GetPlayers()[index].GetEmote();
                        }
                        else board += "❓";
                    }
                    board += "\n";
                }
                return board;
            }

            public string DescribeRoom()
            {
                string msg = "";
                if (Size < 7) msg += "You enter a small room. ";
                else if (Size < 10) msg += "You enter a medium sized room. ";
                else msg += "You enter a large room. ";

                msg += "Inside the room is ";

                Dictionary<string, int> mons = new Dictionary<string, int>();
                foreach (Monster mon in Enemies)
                {
                    if (mons.ContainsKey(mon.GetName()))
                    {
                        mons[mon.GetName()]++;
                    }
                    else mons.Add(mon.GetName(), 1);
                }

                char[] vowels = { 'a', 'e', 'i', 'o', 'u' };

                for (int i = 0; i < mons.Count(); i++)
                {
                    bool isVowel = vowels.Contains(Char.ToLower(mons.ElementAt(i).Key[0]));
                    string vowelN = "";
                    if (isVowel) vowelN = "n";
                    if (i > 0 && i == mons.Count() - 1) msg += "and ";
                    if (mons.ElementAt(i).Value > 1) msg += $"a group of {mons.ElementAt(i).Key.ToTitleCase()}s";
                    else msg += $"a{vowelN} {mons.ElementAt(i).Key.ToTitleCase()}";

                    if (mons.Count() >= 3) msg += ",";
                    msg += " ";
                }
                msg = msg.Trim(',', ' ');
                msg += ".";

                return msg;
            }
        }

                
    }
}
