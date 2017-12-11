using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;

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
        public static void UnPurge(object state)
        {
            Var.purging = false;
            unpurge.Dispose();
        }
    }
}
