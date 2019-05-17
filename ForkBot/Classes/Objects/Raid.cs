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
            static Class[] classes = { new Class("Archer","The sharpshooting master, attacking from a distance, never missing their target.","🏹"),
                                   new Class("Cleric","The magical healer, protecting and buffing their allies.","💉"),
                                   new Class("Mage","The amazing spellcaster, blasting their enemies with elements and more.","📘"),
                                   new Class("Paladin","The religious warrior, smiting their enemies and blessing their allies.","🔨"),
                                   new Class("Rogue", "The stealthy thief, moving quickly and quietly, their enemies won't see them coming.", "🗡"),
                                   new Class("Warrior","The mighty fighter, using a variety of tools and weapons to surpass their foes.","⚔")};
            public static Class[] Classes() { return classes; }
            public static string startMessage()
            {
                string msg = "🧙 Welcome... To The Dungeon of Efrüg!\nFirst you must choose your class, then you may enter the dungeon and duel various beasts, before taking on... ***The Boss!***\n" +
                                         "To start, use the command `;r choose [class]`! Choose between the following classes:\n\n";

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
                    if (s.StartsWith(data + ":")) return s.Split(':')[1];
                }

                string[] newData = { data+":0" };
                profileData = profileData.Concat(newData).ToArray();

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
            
        }
    }
}
