using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using System.IO;

namespace ForkBot
{
    public class Timers
    {
        public static Timer unpurge;
        public static async void UnPurge(object state)
        {
            await Var.purgeMessage.DeleteAsync();
            Var.purging = false;
            unpurge.Dispose();
        }

        public static Timer RemindTimer;
        public static async void Remind(object state)
        {
            if (!File.Exists("Files/userreminders.txt")) File.WriteAllText("Files/userreminders.txt", "");
            string[] reminders = File.ReadAllLines("Files/userreminders.txt");
            bool changed = false;
            for(int i = reminders.Count() - 1; i >= 0; i--)
            {
                //format: user_id//#//reminder//#//datetimeString
                var reminderData = reminders[i].Split(new string[] { "//#//" }, StringSplitOptions.None);
                if (Var.CurrentDate() > Functions.StringToDateTime(reminderData[2]))
                {
                    changed = true;
                    reminders[i] = "";

                    var user = Bot.client.GetUser(Convert.ToUInt64(reminderData[0]));
                    await user.SendMessageAsync(reminderData[1]);
                }
            }

            if (changed)
            {
                File.Delete("Files/userreminders.txt");
                File.WriteAllLines("Files/userreminders.txt", reminders.Where(x => x != ""));
            }
            
        }

        public static Timer BidTimer;
        public static async void Bid(object state)
        {
            string[] posts = File.ReadAllLines("Files/FreeMarket.txt");
            List<string> expired = new List<string>();
            List<string> expiringSoon = new List<string>();
            foreach(string post in posts)
            {
                var expiryDate = Functions.StringToDateTime(post.Split('|')[5]);
                if (expiryDate - Var.CurrentDate() < new TimeSpan(0)) expired.Add(post);
                else if (expiryDate - Var.CurrentDate() < new TimeSpan(1,0,0,0)) expired.Add(post);
            }

            foreach(string warn in expiringSoon)
            {
                //ulong userID = 
            }
        }
    }
}
