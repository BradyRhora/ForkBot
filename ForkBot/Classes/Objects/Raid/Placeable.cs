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
            public bool Acted { get; set; } = false;
            public bool Dead { get; set; } = false;
            public Placeable()
            {
                Actions = new Action[] { Action.Pass, Action.Move, Action.Attack };
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
            public abstract int RollAttackDamage();
            public string TakeDamage(int attackDamage)
            {
                Health -= attackDamage;
                if (Health <= 0)
                {
                    Health = 0;
                    Dead = true;
                    return $"{GetEmote()} {GetName()} falls to the ground, dead.";
                    
                }
                return null;
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
                Health = (Level * 10) + rdm.Next(Level);
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

            public Monster Clone(Game game = null)
            {
                Monster monster = new Monster(Name, Emote, Level, game);
                return monster;
            }

            Placeable FindClosestEnemy()
            {
                var room = Game.GetCurrentRoom();
                int closestIndex = 0;
                int closestDist = GetDistance(room.Players[0]);
                for (int i = 1; i < room.Players.Count(); i++)
                {
                    int dist = GetDistance(room.Players[i]);
                    if (dist < closestDist && !room.Players[i].Dead)
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
                for (int x2 = x - 1; x2 <= x + 1; x2++)
                {
                    for (int y2 = y - 1; y2 <= y + 1; y2++)
                    {
                        if (x == x2 ^ y == y2)
                        {
                            if ((Game.GetCurrentRoom().IsSpaceEmpty(x2, y2) || x2 == X && y2==Y) && x2 >= 0 && y2 >= 0 && x2 < Game.GetCurrentRoom().GetSize() && y2 < Game.GetCurrentRoom().GetSize())
                            {
                                int distance = (GetDistance(x2, y2));
                                if (distance < closest) //make sure its checking right spots and not moving onto player
                                {
                                    closest = distance;
                                    closestCoord = new int[] { x2, y2 };
                                }
                            }
                        }
                    }
                }
                if (closestCoord[0] == x && closestCoord[1] == y) closestCoord = new int[] { -1,-1};
                return closestCoord;
            }

            public override int GetMoveDistance()
            {
                return (Level / 2) + 1;
            }
            public override int RollAttackDamage()
            {
                return rdm.Next(Level*10) + (Level/2);
            }


            public string ChooseAction(Room room)
            {
                var target = FindClosestEnemy();
                string msg = "";
                var coords = GetClosestSide(target.X, target.Y);
                if (GetDistance(target.X, target.Y) - 1 <= GetMoveDistance() && coords[0] != -1)
                {
                    if (!(coords[0] == X && coords[1] == Y))
                    {
                        SetLocation(coords[0], coords[1]);
                        msg += $"{GetEmote()} {GetName()} moves towards {target.GetName()}.";
                    }

                    int attackDMG = RollAttackDamage();
                    msg += $"\n{GetEmote()} {GetName()} attacks {target.GetEmote()} {target.GetName()} for {attackDMG} damage!";
                    var dead = target.TakeDamage(attackDMG);
                    if (dead != null) msg += "\n" + dead;
                }
                else
                {
                    for (int i = 0; i < GetMoveDistance(); i++)
                    {
                        int xDist = Math.Abs(target.X - X);
                        int yDist = Math.Abs(target.Y - Y);
                        bool equDist = xDist == yDist;
                        bool didMove = true;
                        if (equDist)
                        {
                            if (target.X < X && room.IsSpaceEmpty(X - 1, Y)) X--;
                            else if (target.Y < Y && room.IsSpaceEmpty(X, Y - 1)) Y--;
                            else if (target.X > X && room.IsSpaceEmpty(X + 1, Y)) X++;
                            else if (target.Y > Y && room.IsSpaceEmpty(X, Y + 1)) Y++;
                            else didMove = false;
                        }
                        else
                        {
                            if (yDist > xDist)
                            {
                                if (target.Y < Y && room.IsSpaceEmpty(X, Y - 1)) Y--;
                                else if (target.Y > Y && room.IsSpaceEmpty(X, Y + 1)) Y++;
                                else didMove = false;
                            }
                            else
                            {
                                if (target.X < X && room.IsSpaceEmpty(X - 1, Y)) X--;
                                else if (target.X > X && room.IsSpaceEmpty(X + 1, Y)) X++;
                                else didMove = false;
                            }
                        }

                        if (!didMove)
                        {
                            int[] directions = new int[4];
                            for (int j = 0; j < 4; j++) directions[j] = -1;
                            for (int j = 0; j < 4; j++)
                            {
                                int num = rdm.Next(4);
                                if (directions.Contains(num))
                                {
                                    j--;
                                    continue;
                                }
                                else directions[j] = num;
                            }

                            bool moved = false;
                            foreach (int dir in directions)
                            {
                                switch (dir)
                                {
                                    case 0:
                                        if (room.IsSpaceEmpty(X, Y - 1))
                                        {
                                            Y--;
                                            moved = true;
                                        }
                                        break;
                                    case 1:
                                        if (room.IsSpaceEmpty(X + 1, Y))
                                        {
                                            X++;
                                            moved = true;
                                        }
                                        break;
                                    case 2:
                                        if (room.IsSpaceEmpty(X, Y + 1))
                                        {
                                            Y++;
                                            moved = true;
                                        }
                                        break;
                                    case 3:
                                        if (room.IsSpaceEmpty(X - 1, Y))
                                        {
                                            X--;
                                            moved = true;
                                        }
                                        break;
                                }
                                if (moved) break;
                            }
                        }
                    }
                    msg += $"{GetEmote()} {GetName()} moves towards {target.GetName()}.";
                }
                return msg;
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

                while (newAmt > EXPToNextLevel())
                {
                    newAmt = newAmt - EXPToNextLevel();
                    LevelUp();
                }


                SetData("exp", newAmt.ToString());
            }

            void LevelUp()
            {
                throw new NotImplementedException();
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
            public override int RollAttackDamage()
            {
                return rdm.Next(GetClass().Power) + (GetClass().Power / 2); //add weapon damage
            }

            public int EXPToNextLevel()
            {
                return (int)Math.Pow(5, GetLevel());
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

            public async Task<bool> Act(string[] commands)
            {
                string actionS = commands[0];
                Action action = Actions.Where(x => x.Name.ToLower() == actionS.ToLower()).FirstOrDefault();
                if (action == null) return false;
                if (action.Equals(Action.Move))
                {
                    if (StepsLeft <= 0) return false;
                     string direction = commands[1].ToLower();
                    switch (direction)
                    {
                        case "left":
                            if (Game.GetCurrentRoom().IsSpaceEmpty(X-1,Y))
                                X--;
                            else
                                return false;
                            
                            break;
                        case "right":
                             if (Game.GetCurrentRoom().IsSpaceEmpty(X + 1, Y))
                                X++;
                            else
                                return false;
                            break;
                        case "down":
                            if (Game.GetCurrentRoom().IsSpaceEmpty(X, Y+1))
                                Y++;
                            else
                                return false;
                            break;
                        case "up":
                            if (Game.GetCurrentRoom().IsSpaceEmpty(X, Y-1))
                                Y--;
                            else
                                return false;
                            break;
                        default:
                            return false;
                    }
                    StepsLeft--;
                    Moved = true;
                    await Game.GetChannel().SendMessageAsync(Game.ShowCurrentRoom(false));
                }
                else if (action == Action.Attack)
                {
                    string direction = commands[1].ToLower();
                    int[] attackCoords = new int[] { X, Y };
                    switch (direction)
                    {
                        case "left":
                            attackCoords[0]--;
                            break;
                        case "right":
                            attackCoords[0]++;
                            break;
                        case "down":
                            attackCoords[1]++;
                            break;
                        case "up":
                            attackCoords[1]--;
                            break;
                        default:
                            return false;
                    }
                    Placeable target = Game.GetCurrentRoom().GetPlaceableAt(attackCoords[0], attackCoords[1]);
                    if (target == null) await Game.GetChannel().SendMessageAsync($"{GetEmote()} {GetName()} attacks the air to their {direction}.");
                    else
                    {
                        var damage = RollAttackDamage();
                        var dead = target.TakeDamage(damage);
                        string xtraMSG = "";
                        if (dead != null) xtraMSG = "\n" + dead;
                        else if (target.Health <= target.MaxHealth/2) xtraMSG = " It looks pretty hurt!";
                        await Game.GetChannel().SendMessageAsync($"{GetName()} attacks {target.GetName()} for {damage} damage using their {"[weapon]"}" + xtraMSG);
                    }
                    Acted = true;
                }
                else if (action == Action.Pass)
                {
                    Acted = true;
                }

                //await Game.GetChannel().SendMessageAsync($"{GetName()} acts.");
                if (Acted) Game.GetCurrentRoom().NextInitiative();
                return true;
            }
        }

    }
}
