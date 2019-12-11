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
        static Random rdm = new Random();

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
            if (!File.Exists("Files/FreeMarket.txt")) File.Create("Files/FreeMarket.txt");
            string[] posts = File.ReadAllLines("Files/FreeMarket.txt");
            List<string> expired = new List<string>();
            List<string> expiringSoon = new List<string>();
            foreach(string post in posts)
            {
                var expiryDate = Functions.StringToDateTime(post.Split('|')[5]) + new TimeSpan(14,0,0,0);
                if (expiryDate - Var.CurrentDate() < new TimeSpan(0)) expired.Add(post);
                else if (expiryDate - Var.CurrentDate() < new TimeSpan(1, 0, 0, 0))
                {
                    string id = post.Split('|')[0];
                    if (Properties.Settings.Default.warnedFMs == null) Properties.Settings.Default.warnedFMs = new System.Collections.Specialized.StringCollection();
                    if (!Properties.Settings.Default.warnedFMs.Contains(id))
                    {
                        Properties.Settings.Default.warnedFMs.Add(id);
                        Properties.Settings.Default.Save();
                        expiringSoon.Add(post);
                    }

                }
            }
            
            // postID|userID|item|count|cost|dateposted
            //   [0] |  [1] |[2] | [3] | [4]|   [5]
            foreach (string warn in expiringSoon)
            {
                var data = warn.Split('|');
                ulong userID = Convert.ToUInt64(data[1]);
                var user = Bot.client.GetUser(userID);
                string postID = data[0];
                string count = data[3];
                string item = data[2];
                string price = data[4];
                await user.SendMessageAsync("", embed: new InfoEmbed("WARNING: FREE MARKET POSTING EXPIRATION", "This is your one warning that your Free Market posting of " +
                                                                    $"{count} {item} for {price} coins will be expiring in 24 hours. You may remove this posting for a 25% fee, or it will " +
                                                                    $"be auctioned off and the coins will go towards slots.").Build());
            }

            bool removed = false;
            List<string> bids = new List<string>();
            foreach(string expiry in expired)
            {
                var data = expiry.Split('|');
                ulong userID = Convert.ToUInt64(data[1]);
                var user = Bot.client.GetUser(userID);
                string postID = data[0];
                string count = data[3];
                string item = data[2];
                string price = data[4];
                await user.SendMessageAsync("", embed: new InfoEmbed("FREE MARKET POSTING EXPIRATION", "This message is to inform you that your Free Market posting of " +
                                                                    $"{count} {item} for {price} coins has expired. You have not removed it, and now it will " +
                                                                    $"be auctioned off and the coins will go towards slots.").Build());

                if (!File.Exists("Files/BidNotify.txt")) File.Create("Files/BidNotify.txt").Dispose();
                var notifyUsers = File.ReadAllLines("Files/BidNotify.txt").Select(x => Bot.client.GetUser(Convert.ToUInt64(x)));
                foreach (IUser u in notifyUsers)
                {
                    await u.SendMessageAsync("", embed: new InfoEmbed("New Bid Alert", $"There is a new bid for {count} {item}(s)! Get it with the ID: {postID}.\n*You are receiving this message because you have opted in to new bid notifications.*").Build());
                }
                bids.Add(expiry);
                for (int i = 0; i < posts.Count(); i++)
                {
                    if (posts[i].Split('|')[0] == postID)
                    {
                        posts[i] = "";
                        removed = true;
                    }
                }
            }

            if (removed)
            {
                File.WriteAllLines("Files/FreeMarket.txt", posts.Where(x => x != ""));

                if (!File.Exists("Files/Bids.txt")) File.WriteAllText("Files/Bids.txt", "");
                string bidsToAppend = "";
                foreach (string bid in bids)
                {
                    var data = bid.Split('|');
                    bidsToAppend += $"{data[0]}|{data[2]}|{data[3]}|{Functions.DateTimeToString(Var.CurrentDate())}|100|0\n";
                }
                File.AppendAllText("Files/Bids.txt",bidsToAppend);
            }

            if (!File.Exists("Files/Bids.txt")) File.WriteAllText("Files/Bids.txt", "");
            string[] allBids = File.ReadAllLines("Files/Bids.txt");


            bool changed = false;
            for(int i = 0; i < allBids.Count(); i++)
            {
                var data = allBids[i].Split('|');
                var date = Functions.StringToDateTime(data[3]);
                var endTime = (date + new TimeSpan(1, 0, 0, 0)) - Var.CurrentDate();
                if (endTime <= new TimeSpan(0))
                {
                    var item = data[1];
                    var amount = Convert.ToInt32(data[2]);
                    var bidder = Bot.client.GetUser(Convert.ToUInt64(data[5]));
                    var currentBid = Convert.ToInt32(data[4]);
                    if (bidder != null)
                    {
                        await bidder.SendMessageAsync($"Congratulations! You've won the bid for {Functions.GetItemEmote(item)} {amount} {item}(s).");
                        var user = Functions.GetUser(bidder);
                        Properties.Settings.Default.jackpot += currentBid;
                        Properties.Settings.Default.Save();
                        for (int j = 0; j < amount; j++) user.GiveItem(item);
                    }
                    allBids[i] = "";
                    changed = true;
                }
            }

            if (changed) File.WriteAllLines("Files/Bids.txt", allBids.Where(x => x != ""));

            if ((Var.CurrentDate().DayOfWeek == DayOfWeek.Friday || Var.CurrentDate().DayOfWeek == DayOfWeek.Wednesday) && Properties.Settings.Default.lastBid.DayOfYear != Var.CurrentDate().DayOfYear)
            {
                Properties.Settings.Default.lastBid = Var.CurrentDate();
                Properties.Settings.Default.Save();

                var bidItems = new string[] { "key","unicorn","key2","package","pokeball","santa","gift","calling" };
                int amount = rdm.Next(5) + 3;

                string key = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string id = "";

                string item = bidItems[rdm.Next(bidItems.Count())];

                for (int i = 0; i < 5; i++)
                {
                    id += key[rdm.Next(key.Count())];
                }

                string newBid = $"{id}|{item}|{amount}|{Functions.DateTimeToString(Var.CurrentDate())}|100|0\n";

                File.AppendAllText("Files/Bids.txt", newBid);
                var notifyUsers = File.ReadAllLines("Files/BidNotify.txt").Select(x => Bot.client.GetUser(Convert.ToUInt64(x)));
                foreach (IUser u in notifyUsers)
                {
                    await u.SendMessageAsync("", embed: new InfoEmbed("Bi-Weekly Bid Alert", $"The bi-weekly bid is on! This time: {amount} {item}(s)! Get it with the ID: {id}.\n*You are receiving this message because you have opted in to new bid notifications.*").Build());
                }
            }
        }
    }
}
