using System;
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

            string userPath = @"Users\";
            if (File.Exists(userPath + userID + ".user"))
            {
                return new User(userID);
            }
            else
            {
                if (!Directory.Exists("Users"))
                {
                    Directory.CreateDirectory("Users");
                    Console.WriteLine("Created Users folder in bin/Debug/");
                }
                string newUser = "coins:0\nitems{\n}";
                File.WriteAllText(@"Users\" + userID + ".user", newUser);
            }

            return null;
        }

        //returns a users nickname if they have one, otherwise returns their username.
        public static string GetName(IGuildUser user)
        {
            if (user.Nickname == null)
                return user.Username;
            return user.Nickname;
        }
                
        public static string[] GetItemList()
        {
            return File.ReadAllLines("Files/items.txt");
        }

        public static string[] GetBlackMarketItemList()
        {
            return File.ReadAllLines("Files/bmitems.txt");
        }

        public static string GetItemEmote(string item)
        {
            string itemData;
            string[] data;
            try
            {
                if (item.Split('|').Count() > 1) itemData = item;
                else itemData = GetItemData(item);
                data = itemData.Split('|');
                if (data.Count() > 3) return $"<:{data[0]}:{data[3]}>";
                return ":" + data[0] + ":";
            }
            catch (Exception e)
            {
                Console.WriteLine("literally eat my ass (item emote not working, possible incorrect name)");
                return ":question:";
            }
        }
        public static string GetItemData(string item)
        {
            foreach(string data in GetItemList().Concat(GetBlackMarketItemList()))
            {
                if (data.StartsWith(item + "|"))
                    return data;
            }
            return null;
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
        public static async void Respond(IMessage message)
        {
            try
            {
                string msg = Regex.Replace(message.Content, "(<.*@.*377913570912108544.*>)", "").Trim();
                if (msg.ToLower() == "disconnect") msg = "&disconnect=true";
                else msg = "&message=" + msg;
                var xml = web.DownloadString("https://www.botlibre.com/rest/api/form-chat?" +
                                                              "&application=7362540682895337949" +
                                                              "&instance=22180784" +
                                                              $"&conversation={Var.Conversation}" + 
                                                               msg);
                if (msg == "&disconnect=true")
                {
                    Var.Conversation = "0";
                    await message.Channel.SendMessageAsync(":robot::speech_balloon: Goodbye.");
                }
                else
                {
                    XmlDocument response = new XmlDocument();
                    response.LoadXml(xml);
                    var n = response.GetElementsByTagName("message");
                    string responseMsg = n[0].InnerText;
                    if (Var.Conversation == "0") Var.Conversation = response.ChildNodes[1].Attributes[0].Value;
                    responseMsg = Regex.Replace(responseMsg, "(<.*@.*\\w+.*>)", "").Trim();

                    if (message.Author.Id == Constants.Users.FORKPY) responseMsg = message.Author.Mention + " " + responseMsg;
                    else responseMsg = ":robot::speech_balloon: " + responseMsg;

                    if (Var.responding) await message.Channel.SendMessageAsync(responseMsg);
                }
            }
            catch (Exception e)
            {
                if (Var.responding) await message.Channel.SendMessageAsync(":robot::speech_balloon: Watch your profanity!");
                Console.WriteLine(e.Message);
            }
        }
        
        public static KeyValuePair<ulong,int>[] GetTopList(string stat = "")
        {
            var bottom = false;
            if (stat == "bottom")
            {
                bottom = true;
                stat = "";
            }
            var userFiles = Directory.GetFiles(@"Users");
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
            else if (GetItemList().Where(x=>x.Split('|')[0] == stat.ToLower()).Count() > 0)
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
        }

        public static bool CheckUserHasItem(User user, string item, bool remove = true)
        {
            if (user.GetItemList().Contains(item))
            {
                if (remove) user.RemoveItem(item);
                return false;
            }
            return true;
        }

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
    }


    static class Func
    {
        public static string ToTitleCase(this string s)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }
    }
}
