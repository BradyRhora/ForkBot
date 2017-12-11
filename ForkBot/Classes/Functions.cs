using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;

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

        public static void SaveUsers()
        {
            string toWrite = "";
            foreach (User u in Bot.users)
            {
                toWrite += $"{u.ID}|{u.Coins}";
                foreach (string s in u.Items) toWrite += $"|{s}";
                toWrite += "\n";
            }
            File.WriteAllText("Files/users.txt", toWrite);
        }

        public static void LoadUsers()
        {
            foreach (string data in File.ReadLines("Files/users.txt"))
            {
                Bot.users.Add(new User(data: data, load: true));
            }
        }

        public static User GetUser(IUser user) //gets User class for IUser, makes one if there isn't already one.
        {
            int attempts = 0;
            while (attempts < 5)
            {
                foreach (User u in Bot.users) if (u.ID == user.Id) { SaveUsers(); return u; }
                Bot.users.Add(new User(user.Id));
                attempts++;
            }
            return null;
        }
        public static IUser GetUser(User user)
        { 
            return Bot.client.GetUser(user.ID);
        }

        public static void GiveCoins(User u, int amount)
        {
            u.Coins += amount;
            SaveUsers();
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

    }
    

    static class Func
    {
        public static string ToTitleCase(this string s)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
        }
    }
}

/*
 * game ideas
 * emoji guessing?
 * sharades???
 * presents????
 * 
 */
