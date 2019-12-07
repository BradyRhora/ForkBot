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

            public Item Equipped { get; set; }
            public Game Game { get; set; }
            public Action[] Actions;
            public int StepsLeft { get; set; }
            public bool Moved { get; set; } = false;
            public bool Acted { get; set; } = false;
            public bool Dead { get; set; } = false;
            public bool Attackable { get; set; } = true;
            public Placeable()
            {
                Actions = new Action[] { Action.Pass, Action.Move, Action.Attack, Action.Equip };
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
                    return $"☠️ {GetEmote()} {GetName()} falls to the ground, dead. ☠️";
                    
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
            static Monster Giant_Snake = new Monster("giant snake", "🐍", 4);
            static Monster Bat = new Monster("bat", "🦇", 1);
            #endregion
            public readonly static Monster[] Monsters = { Imp, Ghost, Skeleton, Alien, Robot, Spider, Dragon, Mind_Flayer, Snake, Bat, Giant_Snake };

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
                int closestDist = 100;
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
                            if ((Game.GetCurrentRoom().IsSpaceEmpty(x2, y2,false) || x2 == X && y2==Y) && x2 >= 0 && y2 >= 0 && x2 < Game.GetCurrentRoom().GetSize() && y2 < Game.GetCurrentRoom().GetSize())
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
                if (GetDistance(target.X, target.Y) - 1 <= GetMoveDistance() && coords[0] != -1) //if target is within move distance
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

            public int GetDeathEXP()
            {
                return Level * 5;
            }

        }
        public class Profile : Placeable
        {
            public ulong ID;
            string Name;
            string[] profileData;
            IUser DiscordUser;
            public Profile(IUser user)
            {
                Name = Functions.GetName(user as IGuildUser);
                ID = user.Id;
                DiscordUser = user;
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
                DiscordUser = user.DiscordUser;
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

            public Embed BuildProfileEmbed()
            {
                JEmbed emb = new JEmbed();
                emb.ColorStripe = Constants.Colours.YORK_RED;
                Raid.Class rClass = GetClass();
                emb.Author.Name = $"{Functions.GetName(DiscordUser as IGuildUser)} the {rClass.Name}";
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Class";
                    x.Text = rClass.Emote + " " + rClass.Name;
                    x.Inline = false;
                }));
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Level";
                    x.Text = GetLevel().ToString();
                    x.Inline = true;
                }));
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "EXP";
                    x.Text = GetEXP().ToString() + "/" + EXPToNextLevel();
                    x.Inline = true;
                }));
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Emote";
                    x.Text = GetEmote();
                    x.Inline = false;
                }));

                var items = GetItems();
                Dictionary<Raid.Item, int> inv = new Dictionary<Raid.Item, int>();
                for (int i = 0; i < items.Count(); i++)
                {
                    if (inv.ContainsKey(items[i])) inv[items[i]]++;
                    else inv.Add(items[i], 1);
                }
                List<string> fields = new List<string>();
                string txt = "";
                foreach (KeyValuePair<Raid.Item, int> item in inv)
                {
                    string itemListing = $"{item.Key.Emote} {item.Key.GetTagsString()} {item.Key.Name}";
                    if (item.Value > 1) itemListing += $" x{item.Value} ";
                    if (txt.Count() + itemListing.Count() > 1024)
                    {
                        fields.Add(txt);
                        txt = itemListing;
                    }
                    else txt += itemListing + "\n";
                }
                fields.Add(txt);

                string title = "Inventory";
                foreach (string f in fields)
                {
                    emb.Fields.Add(new JEmbedField(x =>
                    {
                        x.Header = title + ":";
                        x.Text = f;
                    }));
                    title += " (cont.)";
                }

                return emb.Build();
            }
            private void Save()
            {
                File.WriteAllLines($"Raid/{ID}.raid", profileData);
            }

            public Item[] GetItems()
            {
                var strItems = GetDataA("inventory");
                List<Item> items = new List<Item>();
                foreach(var i in strItems)
                {
                    var itemData = i.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    string name = itemData[0];
                    string[] tags = new string[0];
                    if (itemData.Count() > 1) tags = itemData[1].Split(',');
                    Item newItem = Item.GetItem(name, tags);
                    items.Add(newItem);
                }
                return items.ToArray();
            }

            public Item GetItemByName(string name)
            {
                var items = GetItems();
                foreach(Item i in items)
                    if (i.Name.ToLower() == name.ToLower()) return i;
                return null;
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
            public void GiveEXP(int amount)
            {
                int current = GetEXP();
                int newAmt = current + amount;

                while (newAmt >= EXPToNextLevel())
                {
                    newAmt = newAmt - EXPToNextLevel();
                    LevelUp();
                }
                
                SetData("exp", newAmt.ToString());
            }

            void LevelUp()
            {
                SetData("level", (Convert.ToInt32(GetLevel()) + 1).ToString());
                //give skill points and stuff
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
                var dmg = rdm.Next(GetClass().Power) + (GetClass().Power / 2); //add weapon damage
                if (Equipped != null) dmg += Equipped.Strength;
                return dmg;
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
            public void GiveItem(Item item)
            {
                string tags = "";
                for (int i = 0; i < item.Tags.Count(); i++) tags += item.Tags[i] + ",";
                tags = tags.Trim(',');
                AddDataA("inventory", tags + item.Name + "|" + tags);
            }

            public string Act(string[] commands)
            {
                string actionS = commands[0];
                Action action = Actions.Where(x => x.Name.ToLower() == actionS.ToLower()).FirstOrDefault();
                if (action == null) return null;
                if (action.Equals(Action.Move))
                {
                    if (StepsLeft <= 0) return null;

                    string direction = commands[1].ToLower();
                    int steps = 1;
                    if (commands.Count() > 2)
                        steps = Convert.ToInt32(commands[2]);

                    string itemMsg = "";
                    bool moveFailed = false;
                    for (int i = 0; i < steps; i++)
                    {
                        switch (direction)
                        {
                            case "left":
                            case "l":
                                if (Game.GetCurrentRoom().IsSpaceEmpty(X - 1, Y, false))
                                    X--;
                                else
                                    moveFailed = true;

                                break;
                            case "right":
                            case "r":
                                if (Game.GetCurrentRoom().IsSpaceEmpty(X + 1, Y, false))
                                    X++;
                                else
                                    moveFailed = true;
                                break;
                            case "down":
                            case "d":
                                if (Game.GetCurrentRoom().IsSpaceEmpty(X, Y + 1, false))
                                    Y++;
                                else
                                    moveFailed = true;
                                break;
                            case "up":
                            case "u":
                                if (Game.GetCurrentRoom().IsSpaceEmpty(X, Y - 1, false))
                                    Y--;
                                else
                                    moveFailed = true;
                                break;
                            default:
                                moveFailed = true;
                                break;
                        }

                        if (moveFailed)
                        {
                            return "You are stopped by something in your way.\n" + Game.ShowCurrentRoom(describe: false);
                        }

                        var item = Game.GetCurrentRoom().GetPlaceableAt(X, Y, type: typeof(Item));
                        
                        if (item != null)
                        {
                            GiveItem((Item)item);
                            Game.GetCurrentRoom().Loot.Remove((Item)item);
                            itemMsg = $"You picked up a(n) {item.GetName()} {item.GetEmote()}.\n";
                        }
                        StepsLeft--;
                        Moved = true;
                    }
                    return itemMsg+Game.ShowCurrentRoom(describe:false);
                }
                else if (action == Action.Attack)
                {
                    string direction = commands[1].ToLower();
                    int[] attackCoords = new int[] { X, Y };
                    switch (direction)
                    {
                        case "left":
                        case "l":
                            attackCoords[0]--;
                            break;
                        case "right":
                        case "r":
                            attackCoords[0]++;
                            break;
                        case "down":
                        case "d":
                            attackCoords[1]++;
                            break;
                        case "up":
                        case "u":
                            attackCoords[1]--;
                            break;
                        default:
                            return null;
                    }
                    Placeable target = Game.GetCurrentRoom().GetPlaceableAt(attackCoords[0], attackCoords[1]);
                    Acted = true;
                    Game.GetCurrentRoom().NextInitiative();
                    if (target == null) return $"{GetEmote()} {GetName()} attacks the air to their {direction}.";
                    else
                    {
                        var damage = RollAttackDamage();
                        var dead = target.TakeDamage(damage);
                        string xtraMSG = "";
                        if (dead != null)
                        {
                            xtraMSG = "\n" + dead;
                            int exp = ((Monster)target).GetDeathEXP();
                            GiveEXP(exp);
                            xtraMSG += " You gained " + exp + " experience.";
                        }
                        else if (target.Health <= target.MaxHealth / 2) xtraMSG = " It looks pretty hurt!";
                        var weapon = Equipped;
                        string weaponName, weaponEmote;
                        if (weapon == null)
                        {
                            weaponName = "Fists";
                            weaponEmote = "✊";
                        }
                        else
                        {
                            weaponName = weapon.Name;
                            weaponEmote = weapon.Emote;
                        }
                        var equippedDmg = 0;
                        if (Equipped != null) equippedDmg = Equipped.Strength;
                        return $"{GetName()} attacks {target.GetName()} for (*roll: {damage - (GetClass().Power / 2) - equippedDmg}* + {GetClass().Power / 2 + equippedDmg}) = **{damage}** damage using their {weaponEmote} {weaponName}." + xtraMSG;
                    }
                }
                else if (action == Action.Equip)
                {
                    if (commands.Count() < 2) return "You must specify an item in your inventory to equip.";
                    string itemStr = commands[1];
                    Item item = GetItemByName(itemStr);
                    if (item != null)
                    {
                        Equipped = item;
                        return $"{GetName()} prepares their {item.GetEmote()} {item.GetName()} for combat.";
                    }
                    else return $"Item not found. Are you sure you have a(n) '{itemStr}'?";
                }
                else if (action == Action.Pass)
                {
                    Acted = true;
                }

                //await Game.GetChannel().SendMessageAsync($"{GetName()} acts.");
                if (Acted) Game.GetCurrentRoom().NextInitiative();
                return "nomsg";
            }
        }
        public class Item : Placeable, IShoppable
        {
            public static Item[] Items =
            {
                new Item("dagger", "🗡", 50, 5, "A short, deadly blade that can be coated in poison.", purchaseable:true),
                new Item("key", "🔑", -1, 1, "An item found in dungeons. Used to open doors and chests."),
                new Item("ring", "💍", 150, 1, "A valuable item that can sold in shops or enchanted.", purchaseable:true),
                new Item("bow and arrow", "🏹", 50, 2, "A well crafted piece of wood with a string attached, used to launch arrows at enemies to damage them from a distance.", purchaseable:true),
                new Item("pill", "💊", 25, 0, "A drug with various effects."),
                new Item("syringe", "💉", 65, 1, "A needle filled with healing liquids to regain health.", purchaseable:true),
                new Item("shield", "🛡", 45, 3, "A sturdy piece of metal that can be used to block incoming attacks.", purchaseable:true),
                new Item("gem", "💎", 200, 0, "A large valuable gem that can be sold or used as an arcane focus to increase a spells power.", purchaseable:true),
                new Item("apple", "🍎", 10, 0, "A red fruit that provides minor healing.", purchaseable:true),
                new Item("banana", "🍌", 12, 0, "A long yellow fruit that provides minor healing.", purchaseable:true),
                new Item("potato", "🥔", 15, 0, "A vegetable that can be cooked in various ways and provides minor healing.", purchaseable:true),
                new Item("meat", "🍖", 20, 0, "Meat from some sort of animal that can be cooked and provides more than minor healing.", purchaseable:true),
                new Item("cake", "🍰", 25, 0,"A baked good, that's usually eaten during celebrations. Provides minor healing for all party members.", purchaseable:true),
                new Item("ale", "🍺", 10, 1, "A cheap drink that provides minor healing, but may have unwanted side effects.", purchaseable:true),
                new Item("guitar", "🎸", 50, 3, "A musical instrument, usually with six strings that play different notes.", purchaseable:true),
                new Item("saxophone", "🎷", 50, 2, "A brass musical instrument.", purchaseable:true),
                new Item("drum", "🥁", 50, 2, "A musical instrument that usually requires sticks to play beats.", purchaseable:true),
                new Item("candle", "🕯", 50, 0, "A chunk of wax with a wick in the middle that slowly burns to create minor light.", purchaseable:true)
            };
            public static Item GetItem(string name, params string[] tags)
            {
                Item i = Items.Where(x => x.Name == name).First().Clone();
                List<Tag> tagList = new List<Tag>();
                foreach(var t in tags) tagList.Add(Tag.GetTagByName(t));
                
                i.Tags = tagList.ToArray();
                return i;
            }

            public string Name { get; set; }
            public string Description { get; set; }
            public int Value { get; }

            public int Strength { get; }
            public string Emote { get; set; }
            public Tag[] Tags;

            //shop vars
            public int Price { get; set; }
            public bool ForSale { get; set; }

            public override string GetEmote()
            {
                return Emote;
            }
            public override int GetMoveDistance()
            {
                throw new NotImplementedException("Item does not implement the method RollAttackDamage()");
            }
            public override int RollAttackDamage()
            {
                throw new NotImplementedException("Item does not implement the method RollAttackDamage()");
            }
            public override string GetName()
            {
                return Name;
            }

            public string[] GetTagNames()
            {
                List<string> names = new List<string>();
                foreach(Tag t in Tags) names.Add(t.Name);
                return names.ToArray();
            }
            public string GetTagsString()
            {
                return string.Join(", ", GetTagNames());
            }
            public Item(string name, string emote, int value, int strength, string description, Tag[] tags = null, bool purchaseable = false)
            {
                Name = name;
                Emote = emote;
                Value = value; 
                Description = description;
                Tags = tags;
                Strength = strength;
                ForSale = purchaseable;
            }
            
            public Item Clone()
            {
                return new Item(Name, Emote, Value, Strength, Description, Tags);
            }

            public static bool operator ==(Item a, Item b) 
            {
                object objA = a, objB = b;
                if (objA == null && objB == null) return true;
                if (objA == null || objB == null) return false;
                return a.Name == b.Name && a.Tags.OrderBy(x=>x).SequenceEqual(b.Tags.OrderBy(x=>x));
            }
            public static bool operator !=(Item a, Item b)
            {
                object objA = a, objB = b;
                if (objA == null && objB == null) return false;
                if (objA == null || objB == null) return true;
                return a.Name != b.Name || !a.Tags.OrderBy(x => x).SequenceEqual(b.Tags.OrderBy(x => x));
            }

            public override bool Equals(object i)
            {
                if (i == null) return false;
                Item b = i as Item;
                return Name == b.Name && Tags.OrderBy(x => x).SequenceEqual(b.Tags.OrderBy(x => x));
            }
            public override int GetHashCode()
            {
                int hash = 13;
                hash = (hash * 7) + Name.GetHashCode();
                if (Tags != null)
                {
                    foreach (Tag tag in Tags)
                    {
                        hash = (hash * 7) + tag.GetHashCode();
                    }
                }
                return hash;
            }
        }

        public class Tag
        {
            public static Tag[] Tags = new Tag[]
            {
                new Tag("golden",5),
                new Tag("sharp",5),
                new Tag("powerful",5)
            };

            public static Tag GetTagByName(string name)
            {
                return Tags.Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
            }

            public string Name;
            public int Chance;

            public Tag(string name, int chance)
            {
                Name = name;
                Chance = chance;
            }

            public Tag Clone()
            {
                return new Tag(Name, Chance);
            }

            public override bool Equals(Object t)
            {
                return Name == (t as Tag).Name;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }
    }
}
