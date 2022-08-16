﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net;
using System.Xml;
using System.Text.RegularExpressions;
using System.Data.SQLite;

namespace ForkBot
{
    public class Functions
    {
        public static Color GetColor(IUser User)
        {
            var user = User as IGuildUser;
            if (user != null)
            {
                if (user.RoleIds.ToArray().Count() > 1)
                {
                    var role = user.Guild.GetRole(user.RoleIds.ElementAtOrDefault(1));
                    return role.Color;
                }
                else return Constants.Colours.DEFAULT_COLOUR;
            }
            else return Constants.Colours.DEFAULT_COLOUR;
        }

        //gets User class for IUser, makes one if there isn't already one.
        public static User GetUser(IUser user)
        {
            return GetUser(user.Id);
        }

        public static User GetUser(ulong userID)
        {
            return new User(userID);
        }

        //returns a users nickname if they have one, otherwise returns their username.
        public static string GetName(IGuildUser user)
        {
            if (user.Nickname == null)
                return user.Username;
            return user.Nickname;
        }
        
        public static ItemTrade GetTrade(IUser user)
        {
            foreach (ItemTrade trade in Var.trades)
            {
                if (trade.HasUser(user))
                {
                    return trade;
                }
            }
            return null;
        }

        
        public static string DateTimeToString(DateTime d)
        {
            return $"{d.Year}:{d.Month}:{d.Day}:{d.Hour}:{d.Minute}";
        }
        public static DateTime StringToDateTime(string s)
        {
            if (s == "0") return new DateTime(0);
            var data = s.Split(':');
            int[] iData = new int[5];
            for (int i = 0; i < 5; i++) iData[i] = Convert.ToInt32(data[i]);
            return new DateTime(iData[0],iData[1],iData[2],iData[3],iData[4],0);
        }
        

        static WebClient web = new WebClient();
        
        string[] stats = { "hygiene", "fashion", "happiness", "fitness", "fullness", "healthiness", "sobriety" };
        /*public static KeyValuePair<ulong,int>[] GetTopList(string stat = "")
        {
            var bottom = false;
            if (stat == "bottom")
            {
                bottom = true;
                stat = "";
            }
            var userFiles = Directory.GetFiles(@"Users").Where(x=>x.EndsWith(".user")).ToArray();
            ulong[] userIDs = new ulong[1];
            
            userIDs = userFiles.Select(x => Convert.ToUInt64(Path.GetFileName(x).Replace(".user", ""))).ToArray();
            
            List<User> users = new List<User>();
            foreach (ulong id in userIDs) try {
                    var u = GetUser(id);
                    users.Add(u);
                }
                catch (Exception) {  }
            Dictionary<ulong, string[]> stats = new Dictionary<ulong, string[]>();
            Dictionary<ulong, int> totalStats = new Dictionary<ulong, int>();
            

            //put users and stats into dictionary
            if (stat == "coins" || stat == "coin")
            {
                foreach (User u in users) totalStats.Add(u.ID, u.GetCoins());
            }
            else if (DBFunctions.GetItemList().Where(x=>x.Split('|')[0] == stat.ToLower()).Count() > 0)
            {
                foreach (User u in users)
                {
                    int itemCount = u.GetItemList().Where(x => x == stat.ToLower()).Count();
                    totalStats.Add(u.ID, itemCount);
                }
            }
            else
            {
                foreach (User u in users) stats.Add(u.ID, u.GetStats());
                for (int i = stats.Count() - 1; i >= 0; i--) if (stats.ElementAt(i).Value.Count() <= 0) stats.Remove(stats.ElementAt(i).Key);
                foreach (var d in stats)
                {
                    int totalStat = 0;
                    foreach (var s in d.Value) if (s.Split(':')[0].Contains(stat)) totalStat += Convert.ToInt32(s.Split(':')[1]);
                    if (!bottom || bottom && totalStat != 0) totalStats.Add(d.Key, totalStat);
                }
            }

            if (stat == "sobriety") bottom = true;
            var list = totalStats.ToList();
            var ordered = list.OrderBy(x => x.Value);
            if (bottom) ordered = list.OrderByDescending(x => x.Value);

            Dictionary<ulong,int> top5 = new Dictionary<ulong, int>();
            int amount;
            if (ordered.Count() >= 5) amount = 5;
            else amount = -(ordered.Count() - 5) - 1;
            for (int i = ordered.Count()-1; i >= ordered.Count()-amount; i--) top5.Add(ordered.ToArray()[i].Key, ordered.ToArray()[i].Value);
            return top5.ToList().ToArray();
        }*/
        
        public static bool Filter(string msg)
        {
            string[] blockedWords = Properties.Settings.Default.blockedWords.Split('|');
            foreach(string word in blockedWords)
            {
                if (word != "")
                    if (msg.ToLower().Contains(word.ToLower())) return true;
            }
            return false;
        }
        
        //splits a message into multiple message when its too long (over 2000 chars)
        public static string[] SplitMessage(string msg)
        {
            List<string> msgs = new List<string>();
            int start = 0;
            for(; msg.Length !=start; )
            {
                //find good spot to split
                int splitIndex = -1;
                if (msg.Length - start < 2000)
                {
                    splitIndex = msg.Length - 1;
                }
                else
                {
                    for (int j = start + 1900; j < start + 1999; j++)
                    {
                        if (msg[j] == '\n')
                        {
                            splitIndex = j;
                            break;
                        }
                    }
                    if (splitIndex == -1)
                        for (int j = start + 1900; j < start + 1999; j++)
                        {
                            if (msg[j] == ' ')
                            {
                                splitIndex = j;
                                break;
                            }
                        }
                    if (splitIndex == -1) splitIndex = start + 1999;
                }
                int end = splitIndex - start;
                msgs.Add(msg.Substring(start, end));
                start = splitIndex + 1;
            }
            return msgs.ToArray();
            
        }
        
        public static string[] GetPokemonList()
        {
            return File.ReadAllLines("Files/pokemon.txt");
        }

        public static string[] GetLegendaryPokemonList()
        {
            return File.ReadAllLines("Files/legendaryPokemon.txt");
        }

        public static async Task<bool> isDM(IMessage message)
        {
            return message.Channel.Name == (await message.Author.CreateDMChannelAsync()).Name;
        }
    }


    static class Func
    {
        public static string ToTitleCase(this string s)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }
    }
}
