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

        public static Timer mvTimer;
        public static IChannel mvChannel;
        public static IReadOnlyCollection<ITextChannel> mvChannels;
        public static IUser[] mvUsers;
        
        public static async void MoveTimer(object state)
        {
            OverwritePermissions op2 = new OverwritePermissions(readMessages: PermValue.Inherit);
            foreach (IGuildChannel c in mvChannels)
            {
                foreach (IUser u in mvUsers)
                {
                    try
                    {
                        if (c != null && c.Id != mvChannel.Id) await c.AddPermissionOverwriteAsync(u, op2);
                    }
                    catch (Exception) { }
                }
            }

            mvTimer.Dispose();
        }

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
            if (!File.Exists("Files/userreminders.txt")) File.Create("Files/userreminders.txt");
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
    }
}
