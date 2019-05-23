using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.IO;

namespace ForkBot
{
    class Raid
    {
        public static bool ChannelHasRaid(IMessageChannel channel)
        {
            return Games.Where(x => x.GetChannel().Id == channel.Id).Count() > 0;
        }
        public static Game GetChannelRaid(IMessageChannel channel)
        {
            return Games.Where(x => x.GetChannel().Id == channel.Id).First();
        }

        public class Class
        {

            static Class Archer = new Class("Archer", "The sharpshooting master, attacking from a distance, never missing their target.", "🏹");
            static Class Cleric = new Class("Cleric", "The magical healer, protecting and buffing their allies.", "💉");
            static Class Mage = new Class("Mage", "The amazing spellcaster, blasting their enemies with elements and more.", "📘");
            static Class Paladin = new Class("Paladin", "The religious warrior, smiting their enemies and blessing their allies.", "🔨");
            static Class Rogue = new Class("Rogue", "The stealthy thief, moving quickly and quietly, their enemies won't see them coming.", "🗡");
            static Class Warrior = new Class("Warrior", "The mighty fighter, using a variety of tools and weapons to surpass their foes.", "⚔");
            static Class[] classes = { Archer, Cleric, Mage, Paladin, Rogue, Warrior};

            public static Class[] Classes() { return classes; }
            public static string startMessage()
            {
                string msg = "🧙 Welcome... To The Dungeon of Efrüg! A mysterious dungeon that shifts its rooms with each entry, full of deadly monsters and fearsome foes!\n"+
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

            public Class(string Name, string Description, string Emote)
            {
                this.Name = Name;
                this.Description = Description;
                this.Emote = Emote;
            }
            public static Class GetClass(string className)
            {
                return classes.Where(x => x.Name.ToLower() == className.ToLower()).FirstOrDefault();
            }
        }

        public static void CreateRaidProfile(User user) => CreateRaidProfile(user.ID); 
        public static void CreateRaidProfile(IUser user) => CreateRaidProfile(user.Id); 
        public static void CreateRaidProfile(ulong id)
        {
            File.Create($"Raid/{id}.raid");
        }

        public class Profile
        {
            public ulong ID;
            string[] profileData;

            public Profile(IUser user)
            {
                ID = user.Id;
                if (!Directory.Exists("Raid")) Directory.CreateDirectory("Raid");
                if (File.Exists($"Raid/{ID}.raid"))
                    profileData = File.ReadAllLines($"Raid/{ID}.raid");
                else
                {
                    CreateRaidProfile(ID);
                    profileData = new string[] { };
                }
            }

            public string GetData(string data)
            {
                foreach(string s in profileData)
                {
                    if (s.ToLower().StartsWith(data.ToLower() + ":")) return s.Split(':')[1];
                }

                string[] newData = { data.ToLower()+":0" };
                profileData = profileData.Concat(newData).ToArray();
                Save();
                return "0";
            }
            public void SetData(string dataName, string data)
            {
                GetData(dataName);
                for(int i = 0; i < profileData.Count(); i++)
                {
                    if (profileData[i].StartsWith(dataName + ":"))
                    {
                        profileData[i] = $"{dataName}:{data}";
                    }
                }

                Save();
            }

            private void Save()
            {
                File.WriteAllLines($"Raid/{ID}.raid",profileData);
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
        }

        public static List<Game> Games = new List<Game>();
        public class Game
        {
            public Profile Host { get; }
            List<Profile> Players = new List<Profile>();
            IMessageChannel Channel;
            public bool Started { get; }

            public Game(Profile host, IMessageChannel chan)
            {
                Host = host;
                Channel = chan;
                Started = false;
            }

            public void Join(Profile user)
            {
                Players.Add(user);
            }

            public IMessageChannel GetChannel()
            {
                return Channel;
            }

            public Profile[] GetPlayers()
            {
                return Players.ToArray();
            }

            public void Kick(Profile user)
            {
                Players.Remove(user);
            }
            
            public async Task Start()
            {
                await Channel.SendMessageAsync("You journey into the mysterious dungeon of Efrüg, knowing not what awaits you and your party...");
            }
        }

        public class Room
        {

        }

        public class Monster
        {
            readonly static Monster[] Monsters =
            {
                
                new Monster("imp","👿"),
                new Monster("ghost","👻"),
                new Monster("skeleton","💀"),
                new Monster("alien","👽"),
                new Monster("robot","🤖"),
                new Monster("spider","🕷"),
                new Monster("dragon","🐉"),
                new Monster("mind flayer","🦑"),
                new Monster("snake","🐍"),
                new Monster("bat","🦇")
            };

            string Name { get; }
            string Emote { get; }

            public Monster(string name, string emote)
            {
                Name = name;
                Emote = emote;
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

            public Item(string name, string emote, int value, string description)
            {
                Name = name;
                Emote = emote;
                Value = value;
                Description = description;
            }

        }

        public class Spell
        {
            static Spell[] Spells =
            {
                new Spell("lightning bolt", "⚡", "Fires a jolt of lightning that can zap through multiple enemies."),
                new Spell("magic missile", "☄", "Multiple beams of light launch at various enemies."),
                new Spell("fire bolt", "🔥", "Launches a burning flame at the enemy, possibly setting it aflame."),
                new Spell("tornado", "🌪", "Creates a powerful cyclone of wind that sucks in enemies and deals damage over time.")
            };

            public Spell(string name, string emote, string description)
            {

            }
        }
    }
}
