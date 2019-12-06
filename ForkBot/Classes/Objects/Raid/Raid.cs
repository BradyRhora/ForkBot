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
            public Profile Host { get; set; }
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
                    msg = $"☠️ {turn.GetEmote()} {turn.GetName()} lies there, dead. ☠️";
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


                if (turn.GetType() == typeof(Monster) || turn.Dead) msg += '\n' + StateCurrentAction();
                return msg;
            }
        }

        public class Room
        {
            Monster[] Enemies;
            int Number;
            public List<Item> Loot = new List<Item>();
            int Size;
            public string[,] Board { get; private set; }
            Game Game;
            public Player[] Players { get; private set; }
            public Player[] AllPlayers { get; private set; }

            public IOrderedEnumerable<KeyValuePair<Placeable, int>> Initiative;
            public int Counter { get; private set; } = 0;
            public bool FirstAction = true;

            public Room(int num, Game game)
            {
                Number = num;
                Game = game;
                Players = Game.GetPlayers();
                AllPlayers = Game.GetPlayers();
                GenerateRoom();
                GenerateEnemies();
                PlacePlayers();
                RollInitiative();
                GenerateLoot();
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
                Players = Players.Where(x => !x.Dead).ToArray();
                for (int i = 0; i < playerCount; i++)
                    Players[i].SetLocation(0, y + i);

                if (Players[0].Y == Size)
                {
                    Console.Write("HUH");
                }
            }

            void GenerateLoot()
            {
                for (int i = 0; i < Size/3; i++)
                {
                    int index = rdm.Next(Item.Items.Count());
                    Loot.Add(Item.Items[index].Clone());
                    int x, y;
                    do
                    {
                        x = rdm.Next(Size);
                        y = rdm.Next(Size);
                    }
                    while (!IsSpaceEmpty(x, y,includeItems:true));
                    Loot[i].SetLocation(x, y);
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
                // remember in array its [Y,X] not [X,Y]
                for (int i = 0; i < Loot.Count(); i++) b2[Loot[i].Y, Loot[i].X] = "loot|" + i;
                for (int i = 0; i < Enemies.Count(); i++) if (Enemies[i].Dead && IsSpaceEmpty(Enemies[i].X,Enemies[i].Y,false) || !Enemies[i].Dead) b2[Enemies[i].Y, Enemies[i].X] = "enemy|" + i;
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
                List<Placeable> placeables = new List<Placeable>();
                for (int i = 0; i < Initiative.Count(); i++) placeables.Add(Initiative.ElementAt(i).Key);
                for (int i = 0; i < Loot.Count(); i++) placeables.Add(Loot[i]);
                foreach (var p in placeables)
                {
                    if (p.X == x && p.Y == y) //check item coords
                        if (alive && !p.Dead || !alive)
                            if (type == null) return p;
                            else if (type == p.GetType()) return p;
                }
                return null;
            }
            public bool IsSpaceEmpty(int x, int y, bool includeDead = true, bool includeItems = false)
            {
                var onboard = x >= 0 && y >= 0 && x < Size && y < Size;

                bool people = true;
                if (includeDead)
                    people = Initiative.Where(o => o.Key.X == x && o.Key.Y == y).Count() <= 0;
                else
                    people = Initiative.Where(o => o.Key.X == x && o.Key.Y == y && !o.Key.Dead).Count() <= 0;

                bool items = true;
                if (includeItems)
                    items = Loot.Where(o => o != null && o.X == x && o.Y == y).Count() <= 0;
                

                var empty = onboard && people && items;
                return empty;
            }
            public int GetSize() { return Size; }
        }

        public class Help
        {
            static Help[] helps =
            {
                new Help("This action is used to move around the board while in game. The syntax is `;r mo&&&ve [dire&&&ction] (amount)`.\n"+
                         "The [direction] parameter is mandatory and specifies which direction to move in. Valid directions include `up`,`down`,`left`,`right`."+
                         "You may also use just the first letter of the direction, which includes `u`,`d`,`l`,`r`.\n"+
                         "The `(amount)` parameter is optional and used when you want to move one direction multiple times. If something blocks your path before "+
                         "you reach your destination, the movement will be stopped.",
                         "move","walk","run"),
                new Help("This action is used to attack other creatures on the board with your currently equipped weapon. The syntax is `;r att&&&ack [dire&&&ction]`"+
                         "The [direction] parameter is mandatory and specifies which direction to attack in. Valid directions include `up`,`down`,`left`,`right`."+
                         "You may also use just the first letter of the direction, which includes `u`,`d`,`l`,`r`.\n",
                         "attack"),
                new Help("","spell"),//
                new Help("The bad guys in the dungeon. Defeat all of them to move onto the next room of the dungeon. As you progress through the dungeon, the monsters "+
                         "will become increasingly powerful. Monsters occasionally drop loot, and sometimes stronger monsters can drop better loot. Stronger "+
                         "monsters can move more spaces in their turn and deal more damage with their attacks. Sometimes certain monsters will have special "+
                         "abilities as well.",
                         "monster","enemy"),
                new Help("","party","host","join","kick","close","start"),//
                new Help("","dungeon"),//
                new Help("","item","weapon","armour","equip"),//
                new Help("","direction"),//
                new Help("","dungeon"),//
                new Help("","emote","emoji","icon"),//
                new Help("","profile","player","exp","experience","level","class")//
            };

            string[] KeyWords;
            string HelpMsg;

            public Help(string msg, params string[] words)
            {
                HelpMsg = msg;
                KeyWords = words;
            }

            public static Help GetHelp(string keyWord)
            {
                foreach (Help h in helps)
                {
                    if (h.KeyWords.Contains(keyWord)) return h;
                }
                return null;
            }

            public Embed BuildHelpEmbed()
            {
                JEmbed emb = new JEmbed();
                emb.Title = KeyWords[0].ToTitleCase();
                List<string> allKeyWords = new List<string>();
                foreach (Help h in helps) foreach (string kw in h.KeyWords) allKeyWords.Add(kw);
                string words = "";
                foreach(string kw in KeyWords.OrderBy(x=>x.ToString()))
                {
                    words += $"`{kw}` ";
                }
                emb.Description = words.Trim();

                string msg = HelpMsg;

                foreach(string kw in allKeyWords)
                {
                    msg = msg.Replace(kw, $"**{kw}**");
                }
                msg = msg.Replace("&&&", "");
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Description";
                    x.Text = msg;
                }));
                emb.ColorStripe = Constants.Colours.YORK_RED;
                emb.Footer.Text = "Bolded words have their own help pages. Use `;r help [word]` for more information on them.";
                return emb.Build();
            }

            public static Embed ShowAllHelps()
            {
                IOrderedEnumerable<Help> orderedHelps = helps.OrderBy(x => x.KeyWords[0]);
                JEmbed emb = new JEmbed();
                emb.Title = "Raid Help";
                emb.Description = "Use `;r help [topic]` to get more information on the inputted topic.";
                string topics = "";
                foreach(Help h in orderedHelps)
                {
                    topics += $"`{h.KeyWords[0]}`, ";
                }
                topics = topics.Trim(',', ' ');
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Topics";
                    x.Text = topics;
                }));
                emb.ColorStripe = Constants.Colours.YORK_RED;
                return emb.Build();
            }
        }
    }
}
