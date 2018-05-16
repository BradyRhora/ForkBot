using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
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
            if (user == null)
            {
                if (user.RoleIds.ToArray().Count() > 1)
                {
                    var role = Bot.client.GetGuild(Constants.Guilds.YORK_UNIVERSITY).GetRole(user.RoleIds.ElementAtOrDefault(1));
                    return role.Color;
                }
                else return Constants.Colours.DEFAULT_COLOUR;
            }
            else return Constants.Colours.DEFAULT_COLOUR;
        }
        
        public static User GetUser(IUser user) //gets User class for IUser, makes one if there isn't already one.
        {
            /*int attempts = 0;
            while (attempts < 5)
            {
                foreach (User u in Bot.users) if (u.ID == user.Id) { SaveUsers(); return u; }
                Bot.users.Add(new User(user.Id));
                attempts++;
            }*/

            string userPath = @"Users\";
            if (File.Exists(userPath + user.Id + ".user"))
            {
                return new User(user.Id);
            }
            else
            {
                string newUser = "coins:0\nitems{\n}";
                File.WriteAllText(@"Users\" + user.Id + ".user",newUser);
            }

            return null;
        }

        public static User GetUser(ulong userID)
        {
            return GetUser(Bot.client.GetUser(userID));
        }
        
        public static string GetTID(string html)
        {
            var c = html.ToCharArray();
            int start = 0, end = 0;
            for (int i = 0; i < c.Count(); i++)
            {
                if (new String(c, i, 4) == "tid=")
                {
                    start = i + 4;
                    break;
                }
            }

            for (int i = start; i < c.Count(); i++)
            {
                if (!Char.IsNumber(c[i]))
                {
                    end = i;
                    break;
                }
            }
            int length = end - start;
            return html.Substring(start, length);
        }

        public static async Task SendAnimation(IMessageChannel chan, EmoteAnimation anim) { await SendAnimation(chan, anim, ""); }

        static IUserMessage animation;
        static EmoteAnimation anim;
        static string varEmote;
        static int frameCount;
        static Timer animTimer;
        public static async Task SendAnimation(IMessageChannel chan, EmoteAnimation Animation, string var)
        {
            anim = Animation;
            varEmote = var;
            frameCount = 1;
            animation = await chan.SendMessageAsync(anim.frames[0].Replace("%", varEmote));
            animTimer = new Timer(new TimerCallback(AnimateTimerCallback), null, 1000, 1000);
        }

        static async void AnimateTimerCallback(object state)
        {
            await animation.ModifyAsync(x => x.Content = anim.frames[frameCount].Replace("%", varEmote));
            frameCount++;
            if (frameCount >= anim.frames.Count())
            {
                Var.timerComplete = true;
                animTimer.Dispose();
            }
        }

        public static string[] GetItemList()
        {
            return File.ReadAllLines("Files/items.txt");
        }
        public static string[] GetRareItemList()
        {
            return File.ReadAllLines("Files/rareitems.txt");
        }
        public static string GetItemEmote(string itemData)
        {
            var data = itemData.Split('|');
            if (data.Count() > 3) return $"<:{data[0]}:{data[3]}>";
            return ":" + data[0] + ":";
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
                    if (Var.responding) await message.Channel.SendMessageAsync(":robot::speech_balloon: " + responseMsg);
                }
            }
            catch (Exception e)
            {
                if (Var.responding) await message.Channel.SendMessageAsync(":robot::speech_balloon: Watch your profanity!");
                Console.WriteLine(e.Message);
            }
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
