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
                toWrite += $"{u.Username}|{u.ID}|{u.Coins}\n";
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
    }
}
