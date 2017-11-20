using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
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
            foreach(User u in Bot.users)
            {
                toWrite += $"{u.ID}|{u.Coins}\n";
            }
            File.WriteAllText("Files/users.txt", toWrite);
        }
        
        public static void LoadUsers()
        {
            foreach(string data in File.ReadLines("Files/users.txt"))
            {
                Bot.users.Add(new User(data:data,load:true));
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

        
    }

    static class func
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
