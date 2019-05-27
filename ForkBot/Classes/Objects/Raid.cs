using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.IO;

namespace ForkBot
{
    public class Raid
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

        public class Class
        {

            static Class Archer = new Class("Archer", "The sharpshooting master, attacking from a distance, never missing their target.", "🏹", power: 7);
            static Class Cleric = new Class("Cleric", "The magical healer, protecting and buffing their allies.", "💉", magic_power: 6);
            static Class Mage = new Class("Mage", "The amazing spellcaster, blasting their enemies with elements and more.", "📘", magic_power: 8);
            static Class Paladin = new Class("Paladin", "The religious warrior, smiting their enemies and blessing their allies.", "🔨", magic_power: 7, power: 6);
            static Class Rogue = new Class("Rogue", "The stealthy thief, moving quickly and quietly, their enemies won't see them coming.", "🗡", speed: 8);
            static Class Warrior = new Class("Warrior", "The mighty fighter, using a variety of tools and weapons to surpass their foes.", "⚔", power: 8);
            static Class[] classes = { Archer, Cleric, Mage, Paladin, Rogue, Warrior };

            public static Class[] Classes() { return classes; }
            public static string startMessage()
            {
                string msg = "🧙 Welcome... To The Dungeon of Efrüg! A mysterious dungeon that shifts its rooms with each entry, full of deadly monsters and fearsome foes!\n" +
                             "First you must choose your class, then you may enter the dungeon and duel various beasts, before taking on... ***The Boss!***\n" +
                             "To start, use the command `;r choose [class]`! You may choose between the following classes:\n\n";

                foreach (Class c in classes)
                {
                    msg += $"{c.Emote} The {c.Name}, {c.Description}\n";
                }



                return msg;
            }

            public string Name;
            public string Description;
            public string Emote;
            public int Speed, Power, Magic_Power;
            public Class(string Name, string Description, string Emote, int speed = 5, int power = 5, int magic_power = 5)
            {
                this.Name = Name;
                this.Description = Description;
                this.Emote = Emote;
                Speed = speed;
                Power = power;
                Magic_Power = magic_power;
            }
            public static Class GetClass(string className)
            {
                return classes.Where(x => x.Name.ToLower() == className.ToLower()).FirstOrDefault();
            }
        }

        public class Profile : Placeable
        {
            public ulong ID;
            string Name;
            string[] profileData;
            public Profile(IUser user)
            {
                Name = Functions.GetName(user as IGuildUser);
                ID = user.Id;
                if (!Directory.Exists("Raid")) Directory.CreateDirectory("Raid");
                if (File.Exists($"Raid/{ID}.raid")) profileData = File.ReadAllLines($"Raid/{ID}.raid");
                else
                {
                    profileData = new string[] { };
                    Save();
                }
            }

            public Profile(Profile user)
            {
                ID = user.ID;
                profileData = user.profileData;
                Name = user.GetName();
            }

            public string GetData(string data)
            {
                foreach (string s in profileData)
                {
                    if (s.ToLower().StartsWith(data.ToLower() + ":")) return s.Split(':')[1];
                }

                string[] newData = { data.ToLower() + ":0" };
                profileData = profileData.Concat(newData).ToArray();
                Save();
                return "0";
            }
            public void SetData(string dataName, string data)
            {
                GetData(dataName);
                for (int i = 0; i < profileData.Count(); i++)
                {
                    if (profileData[i].StartsWith(dataName + ":"))
                    {
                        profileData[i] = $"{dataName}:{data}";
                        break;
                    }
                }

                Save();
            }
            public void SetMultipleData(string[,] data)
            {
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    GetData(data[i, 0]);
                    for (int j = 0; j < profileData.Count(); j++)
                    {
                        if (profileData[j].StartsWith(data[i, 0] + ":"))
                        {
                            profileData[j] = $"{data[i, 0]}:{data[i, 1]}";
                            break;
                        }
                    }
                }
                Save();
            }

            public string[] GetDataA(string data)
            {
                var uData = profileData;
                List<string> results = new List<string>();
                bool adding = false;
                foreach (string d in uData)
                {
                    if (d.StartsWith(data)) adding = true;
                    else if (adding && d.Contains("}")) break;
                    else if (adding) results.Add(d.Replace("\t", ""));
                }

                if (!adding)
                {
                    var list = uData.ToList();
                    list.Add($"{data}{{");
                    list.Add("}");
                    profileData = list.ToArray();
                    Save();
                }

                return results.ToArray();
            }
            public void AddDataA(string dataA, string data)
            {
                GetDataA(dataA); //ensure data array exists
                string[] newData = new string[profileData.Count() + 1];
                for (int i = 0; i < profileData.Count(); i++)
                {
                    if (profileData[i].Contains($"{dataA}{{"))
                    {
                        for (int o = 0; o <= i; o++) newData[o] = profileData[o];

                        newData[i + 1] = "\t" + data;

                        for (int o = i + 2; o < newData.Count(); o++) newData[o] = profileData[o - 1];
                        break;
                    }

                }
                profileData = newData;
                Save();
            }
            public void RemoveDataA(string dataA, string data)
            {
                var newData = new string[profileData.Count() - 1];
                bool removing = false;
                for (int i = 0; i < profileData.Count(); i++)
                {
                    if (profileData[i].Contains($"{dataA}{{")) removing = true;
                    else if (removing && profileData[i].Contains(data))
                    {
                        for (int o = 0; o < i; o++) newData[o] = profileData[o];
                        for (int o = i + 1; o <= newData.Count(); o++) newData[o - 1] = profileData[o];
                        break;
                    }
                }
                profileData = newData;
                Save();
            }

            private void Save()
            {
                File.WriteAllLines($"Raid/{ID}.raid", profileData);
            }

            public Class GetClass()
            {
                return Class.GetClass(GetData("class"));
            }
            public int GetLevel()
            {
                return Convert.ToInt32(GetData("level"));
            }
            public int GetEXP()
            {
                return Convert.ToInt32(GetData("exp"));
            }

            public void AddEXP(int amount)
            {
                int current = GetEXP();
                int newAmt = current + amount;
                SetData("exp", newAmt.ToString());
            }

            public override string GetName()
            {
                return Name;
            }
            public override string GetEmote()
            {
                return GetData("emote");
            }
            public override int GetMoveDistance()
            {
                return (GetClass().Speed / 2) + 1;
            }
        }

        public class Player : Profile
        {
            Class Class;
            public Player(IUser user, Game game) : base(user) => Initialize(game);
            public Player(Profile user, Game game) : base(user) => Initialize(game);

            void Initialize(Game game)
            {
                Class = GetClass();
                MaxHealth = GetClass().Power * 10;
                Health = MaxHealth;
                Game = game;
            }

            public void Act(string[] commands)
            {
                Game.GetChannel().SendMessageAsync($"{GetName()} acts.");
                Game.GetCurrentRoom().Counter++;
            }
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
                await ShowCurrentRoom();
                await StateCurrentAction();
            }

            public Room GetCurrentRoom()
            {
                return Dungeon[currentRoom];
            }
            async Task ShowCurrentRoom()
            {
                var room = GetCurrentRoom();
                await Channel.SendMessageAsync(room.BuildBoard());
                await Channel.SendMessageAsync(room.DescribeRoom());
            }

            public Placeable GetCurrentTurn()
            {
                var room = GetCurrentRoom();
                var turn = room.Initiative.ElementAt(room.Counter).Key;
                return turn;
            }

            public async Task StateCurrentAction()
            {
                var room = GetCurrentRoom();
                var turn = GetCurrentTurn();

                string msg = "";
                if (turn.GetType() == typeof(Player))
                {
                    var player = (Player)turn;
                    msg += $"It's {player}'s turn.\nChoose one of the folling actions with `;r act [#]`\n```\n";
                    var actions = player.GetActions();
                    for (int i = 0; i < actions.Count(); i++)
                    {
                        msg += $"[{i+1}] {actions[i].Name}: {actions[i].Description}\n";
                    }
                    msg += "```";
                }
                else if (turn.GetType() == typeof(Monster))
                {
                    var monster = (Monster)turn;
                    msg = monster.ChooseAction(room);
                    room.Counter++;
                }

                await Channel.SendMessageAsync(msg);
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
            public int Counter = 0;

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
            }

            void RollInitiative()
            {
                var rolls = new Dictionary<Placeable, int>();
                foreach (Player p in Players)
                {
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

            public string BuildBoard()
            {
                var b2 = Board;
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

        public class Monster : Placeable
        {
            #region Monsters
            static Monster Imp = new Monster("imp", "👿", 2);
            static Monster Ghost = new Monster("ghost", "👻", 5);
            static Monster Skeleton = new Monster("skeleton", "💀", 3);
            static Monster Alien = new Monster("alien", "👽", 6);
            static Monster Robot = new Monster("robot", "🤖", 5);
            static Monster Spider = new Monster("Giant Spider", "🕷", 3);
            static Monster Dragon = new Monster("dragon", "🐉", 10);
            static Monster Mind_Flayer = new Monster("mind flayer", "🦑", 7);
            static Monster Snake = new Monster("snake", "🐍", 1);
            static Monster GiantSnake = new Monster("giant snake", "🐍", 4);
            static Monster Bat = new Monster("bat", "🦇", 1);
            #endregion
            public readonly static Monster[] Monsters = { Imp, Ghost, Skeleton, Alien, Robot, Spider, Dragon, Mind_Flayer, Snake, Bat };

            string Name;
            string Emote;
            public int Level { get; }
            public Monster(string name, string emote, int level, Game game = null)
            {
                Name = name;
                Emote = emote;
                Level = level;
                Game = game;
            }
            
            public override string GetEmote()
            {
                return Emote;
            }
            public override string GetName()
            {
                return Name;
            }

            public Monster Clone()
            {
                Monster monster = new Monster(Name, Emote, Level);
                return monster;
            }

            Placeable FindClosestEnemy(Room room)
            {
                int closestIndex = 0;
                int closestDist = GetDistance(room.Players[0]);
                for(int i = 1; i < room.Players.Count(); i++)
                {
                    int dist = GetDistance(room.Players[i]);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestIndex = i;
                    }
                }
                return room.Players[closestIndex];
            }

            int GetDistance(int x, int y)
            {
                return Math.Abs(X - x) + Math.Abs(Y - y);
            }
            int[] GetClosestSide(int x, int y)
            {
                int closest = GetDistance(x, y);
                int[] closestCoord = { x, y };
                for (int x2 = x-1; x2 <= x+1; x++)
                {
                    for (int y2 = y - 1; y2 <= y + 1; y++)
                    {
                        if (x==x2 ^ y == y2)
                        {
                            closest = GetDistance(x2, y2);
                            closestCoord = new int[] { x2, y2 };
                        }
                    }
                }
                return closestCoord;
            }

            public override int GetMoveDistance()
            {
                return (Level / 2) + 1;
            }

            public string ChooseAction(Room room)
            {
                var player = FindClosestEnemy(room);
                if (GetDistance(player.X,player.Y)-1 <= GetMoveDistance())
                {
                    var coords = GetClosestSide(player.X, player.Y);
                    SetLocation(coords[0], coords[1]);
                    return $"{GetName()} moves towards {player.GetName()}.";
                }
                return $"{GetName()} waits patiently.";
            }

        }

        public class Action
        {
            public static Action Weapon = new Action("weapon", "Use your fists or currently eqipped weapon to attack an enemy in range.", Type.Attack);
            public static Action Move = new Action("move", "Move somewhere else on the board.", Type.Movement);
            public static Action Spell = new Action("spell", "Cast a spell.", Type.Spell);
            public static Action Pass = new Action("pass", "End your turn without taking an action.", Type.Pass);
            public static Action[] Actions = new Action[] { Weapon,Move,Spell,Pass };
            public string Name;
            public string Description;
            Type type;

            public Action(string name, string description, Type type)
            {
                Name = name;
                Description = description;
                this.type = type;
            }


            public enum Type
            {
                Attack,
                Movement,
                Spell,
                Pass
            }
        }

        public class Item
        {
            readonly static Item[] Items =
            {
                new Item("dagger", "🗡", 50, "A short, deadly blade that can be coated in poison."),
                new Item("key", "🔑", -1, "An item found in dungeons used to open doors and chests."),
                new Item("ring", "💍", 150, "A valuable item that can sold in shops or enchanted."),
                new Item("bow and arrow", "🏹", 50, "A well crafted piece of wood with a string attached, used to launch arrows at enemies to damage them from a distance."),
                new Item("pill", "💊", 25, "A drug with various effects."),
                new Item("syringe", "💉", 65, "A needle filled with healing liquids to regain health."),
                new Item("shield", "🛡", 45, "A sturdy piece of metal that can be used to block incoming attacks."),
                new Item("gem", "💎", 200, "A large valuable gem that can be sold or used as an arcane focus to increase a spells power."),
                new Item("apple", "🍎", 10, "A red fruit that provides minor healing."),
                new Item("banana", "🍌", 12, "A long yellow fruit that provides minor healing."),
                new Item("potato", "🥔", 15, "A vegetable that can be cooked in various ways and provides minor healing."),
                new Item("meat", "🍖", 20, "Meat from some sort of animal that can be cooked and provides more than minor healing."),
                new Item("cake", "🍰", 25, "A baked good, that's usually eaten during celebrations. Provides minor healing for all party members."),
                new Item("ale", "🍺", 10, "A cheap drink that provides minor healing, but may have unwanted side effects."),
                new Item("guitar", "🎸", 50, "A musical instrument, usually with six strings that play different notes."),
                new Item("saxophone", "🎷", 50, "A brass musical instrument."),
                new Item("drum", "🥁", 50, "A musical instrument that usually requires sticks to play beats."),
                new Item("candle", "🕯", 50, "A chunk of wax with a wick in the middle that slowly burns to create minor light.")
            };

            string Name { get; }
            string Description { get; }
            int Value { get; }
            string Emote { get; }
            string[] Tags;

            public Item(string name, string emote, int value, string description, params string[] tags)
            {
                Name = name;
                Emote = emote;
                Value = value;
                Description = description;
                Tags = tags;
            }

        }

        public class Spell
        {
            static Spell Lightning_Bolt = new Spell("lightning bolt", "⚡", "Fires a jolt of lightning that can zap through multiple enemies.");
            static Spell Magic_missile = new Spell("magic missile", "☄", "Multiple beams of light launch at various enemies.");
            static Spell Fire_Bolt = new Spell("fire bolt", "🔥", "Launches a burning flame at the enemy, possibly setting it aflame.");
            static Spell Tornado = new Spell("tornado", "🌪", "Creates a powerful cyclone of wind that sucks in enemies and deals damage over time.");
            static Spell Summon_Familiar = new Spell("summon familiar", "🐺", "Summons a creature to aid you in battle!");
            static Spell[] Spells = { Lightning_Bolt, Magic_missile, Fire_Bolt, Tornado, Summon_Familiar };

            string Name { get; }
            string Emote { get; }
            string Description { get; }
            public Spell(string name, string emote, string description)
            {
                Name = name;
                Emote = emote;
                Description = description;

            }
        }

        public abstract class Placeable
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Health { get; set; }
            public int MaxHealth { get; set; }
            public Game Game { get; set; }
            public Action[] Actions;

            public Placeable()
            {
                Actions = new Action[] { Action.Pass, Action.Move, Action.Weapon};
            }

            public int GetDistance(Placeable p)
            {
                return (int)(Math.Pow(Math.Abs(X - p.X),2) + Math.Pow(Math.Abs(Y - p.Y),2));
            }

            public Action[] GetActions() { return Actions; }
            public void SetLocation(int x, int y)
            {
                X = x;
                Y = y;
            }

            public abstract string GetName();
            public abstract string GetEmote();
            public abstract int GetMoveDistance();
        }
    
    }
}
