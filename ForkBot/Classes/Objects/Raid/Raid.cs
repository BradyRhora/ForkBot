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
                Dungeon[0] = new Room(1, this);
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


                if (turn.Dead)
                {
                    msg = $"{turn.GetEmote()} {turn.GetName()} lies there, dead.";
                    room.NextInitiative();
                }
                else if (turn.GetType() == typeof(Player))
                {
                    var player = (Player)turn;
                    if (!turn.Moved)
                    {
                        msg += room.BuildBoard();
                        if (room.FirstAction)
                        {
                            msg += room.DescribeRoom() + "\n";
                            room.FirstAction = false;
                        }
                        msg += $"It's {player.GetEmote()} <@{player.ID}>'s turn.\nChoose one of the following actions with `;r [action]`. Don't forget to specify a direction if necessary.\n" +
                               $"You can move `{player.StepsLeft}` more spaces. `Health: {player.Health}/{player.MaxHealth}` \n```\n";
                        var actions = player.GetActions();
                        for (int i = 0; i < actions.Count(); i++)
                        {
                            msg += $"{actions[i].Name}: {actions[i].Description}\n";
                        }
                        msg += "```";
                    }
                    else msg += $"You can move `{player.StepsLeft}` more spaces.";
                }
                else if (turn.GetType() == typeof(Monster))
                {
                    var monster = (Monster)turn;
                    msg = monster.ChooseAction(room);
                    room.NextInitiative();
                }

                
                if (Players.Where(x=>x.Dead).Count() == Players.Count())
                {
                    msg = "All players have died. Game over.";
                    Games.Remove(this);
                    return msg;
                }
                else if (room.Initiative.Where(x=>x.Key.GetType() == typeof(Monster) && x.Key.Dead).Count() == room.Initiative.Where(x=>x.Key.GetType() == typeof(Monster)).Count())
                {
                    msg = "All enemies have died. Moving on the next room.\n";
                    currentRoom++;
                    Dungeon[currentRoom] = new Room(currentRoom+1, this);
                    msg += StateCurrentAction();
                    return msg;
                }


                if (turn.GetType() == typeof(Monster) || turn.Dead) msg += '\n' + StateCurrentAction(); //not so sure about this line, why does it use `turn` again?
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
            public bool FirstAction = true;

            public Room(int num, Game game)
            {
                Number = num;
                Game = game;
                Players = Game.GetPlayers();
                GenerateRoom();
                GenerateEnemies();
                PlacePlayers();
                GenerateLoot();
                RollInitiative();
            }

            void GenerateRoom()
            {
                Size = 6;// rdm.Next(5, 10 + Number / 2);
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
                    Enemies[i] = possibleEnemies[rdm.Next(possibleEnemies.Count())].Clone(Game);

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

            void GenerateLoot()
            {
                Loot = new Item[Size / 3];
                for (int i = 0; i < Size/3; i++)
                {
                    int index = rdm.Next(Item.Items.Count());
                    Loot[i] = Item.Items[index].Clone();
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
                turn.Acted = false;
                turn.Moved = false;
                turn.StepsLeft = turn.GetMoveDistance();
            }

            public string BuildBoard()
            {
                var b2 = (string[,])Board.Clone();

                for (int i = 0; i < Enemies.Count(); i++) if (Enemies[i].Dead && IsSpaceEmpty(Enemies[i].X,Enemies[i].Y,false) || !Enemies[i].Dead) b2[Enemies[i].Y, Enemies[i].X] = "enemy|" + i;
                for (int i = 0; i < Players.Count(); i++) b2[Players[i].Y, Players[i].X] = "player|" + i;
                for (int i = 0; i < Loot.Count(); i++) b2[Loot[i].X, Loot[i].Y] = "loot|" + i;

                    string board = "";
                for (int x = 0; x < Size; x++)
                {
                    for (int y = 0; y < Size; y++)
                    {
                        if (b2[x, y] == "empty") board += "⬛";
                        else if (b2[x, y].StartsWith("enemy|"))
                        {
                            int index = Convert.ToInt32(b2[x, y].Split('|')[1]);
                            if (Enemies[index].Dead) board += "☠"; // (skull and crossbones)
                            else board += Enemies[index].GetEmote();
                        }
                        else if (b2[x, y].StartsWith("player|"))
                        {
                            int index = Convert.ToInt32(b2[x, y].Split('|')[1]);
                            if (Game.GetPlayers()[index].Dead) board += "☠"; // '' ''
                            else board += Game.GetPlayers()[index].GetEmote();
                        }
                        else if (b2[x, y].StartsWith("loot|"))
                        {
                            int index = Convert.ToInt32(b2[x, y].Split('|')[1]);
                            board += Loot[index].GetEmote();
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

            public Placeable GetPlaceableAt(int x, int y, bool alive = true, Type type = null)
            {
                Placeable[] placeables = ((Dictionary<Placeable, int>)Initiative).Keys.ToArray(); //set this fr
                for (int i = 0; i < Initiative.Count(); i++) placeables[i] = Initiative.ElementAt(i).Key; // add loot
                foreach (var p in placeables)
                {
                    if (p.X == x && p.Y == y)
                        if (alive && !p.Dead || !alive)
                            if (type == null) return p;
                            else if (type == p.GetType()) return p;
                }
                return null;
            }
            public bool IsSpaceEmpty(int x, int y, bool includedead = true)
            {
                var onboard = x >= 0 && y >= 0 && x < Size && y < Size;
                bool people = false;
                if (includedead)
                    people = Initiative.Where(o => o.Key.X == x && o.Key.Y == y).Count() <= 0;
                else
                    people = Initiative.Where(o => o.Key.X == x && o.Key.Y == y && !o.Key.Dead).Count() <= 0;
                var empty = onboard && people;
                return empty;
            }
            public int GetSize() { return Size; }
        }

                
    }
}
