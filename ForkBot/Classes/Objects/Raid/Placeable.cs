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
        public abstract class Placeable
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Health { get; set; }
            public int MaxHealth { get; set; }
            public Game Game { get; set; }
            public Action[] Actions;
            public int StepsLeft { get; set; }
            public bool Moved { get; set; } = false;
            public Placeable()
            {
                Actions = new Action[] { Action.Pass, Action.Move, Action.Weapon };
            }

            public int GetDistance(Placeable p)
            {
                return (int)(Math.Pow(Math.Abs(X - p.X), 2) + Math.Pow(Math.Abs(Y - p.Y), 2));
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
                for (int i = 1; i < room.Players.Count(); i++)
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
                for (int x2 = x - 1; x2 <= x + 1; x++)
                {
                    for (int y2 = y - 1; y2 <= y + 1; y++)
                    {
                        if (x == x2 ^ y == y2)
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
                if (GetDistance(player.X, player.Y) - 1 <= GetMoveDistance())
                {
                    var coords = GetClosestSide(player.X, player.Y);
                    SetLocation(coords[0], coords[1]);
                    return $"{GetName()} moves towards {player.GetName()}.";
                }
                return $"{GetName()} waits patiently.";
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
            bool Acted = false;
            public Player(IUser user, Game game) : base(user) => Initialize(game);
            public Player(Profile user, Game game) : base(user) => Initialize(game);

            void Initialize(Game game)
            {
                Class = GetClass();
                MaxHealth = GetClass().Power * 10;
                Health = MaxHealth;
                Game = game;
            }

            public async Task<bool> Act(string[] commands)
            {
                string actionS = commands[1];
                Action action = Actions.Where(x => x.Name.ToLower() == actionS.ToLower()).FirstOrDefault();
                if (action == null) return false;
                if (action.Equals(Action.Move))
                {
                    if (StepsLeft <= 0) return false;
                    StepsLeft--;
                    string direction = commands[2].ToLower();
                    switch (direction)
                    {
                        case "left":
                            X--;
                            break;
                        case "right":
                            X++;
                            break;
                        case "down":
                            Y++;
                            break;
                        case "up":
                            Y--;
                            break;
                        default:
                            return false;
                    }
                    Moved = true;
                    await Game.GetChannel().SendMessageAsync(Game.ShowCurrentRoom(false));
                }
                else Acted = true;

                //await Game.GetChannel().SendMessageAsync($"{GetName()} acts.");
                if (Acted) Game.GetCurrentRoom().NextInitiative();
                return true;
            }
        }

    }
}
