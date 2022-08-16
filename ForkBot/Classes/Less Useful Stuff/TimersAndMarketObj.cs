using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using System.IO;
using System.Data.SQLite;

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
            var reminders = Reminder.GetAllReminders();

            for (int i = reminders.Count() - 1; i >= 0; i--)
            {
                
                if (Var.CurrentDate() > reminders[i].RemindTime)
                {
                    var user = reminders[i].User;
                    await user.SendMessageAsync(reminders[i].Text);
                    reminders[i].Delete();
                }
            }
        }

        public static Timer BidTimer;
        public static async void BidTimerCallBack(object state)
        {
            var posts = MarketPost.GetAllPosts();
            List<MarketPost> expired = new List<MarketPost>();
            List<MarketPost> expiringSoon = new List<MarketPost>();
            foreach (var post in posts)
            {
                var expiryDate = post.DatePosted + new TimeSpan(14, 0, 0, 0);
                if (expiryDate - Var.CurrentDate() < new TimeSpan(0)) expired.Add(post);
                else if (expiryDate - Var.CurrentDate() < new TimeSpan(1, 0, 0, 0))
                {
                    string id = post.ID;
                    if (Properties.Settings.Default.warnedFMs == null) Properties.Settings.Default.warnedFMs = new System.Collections.Specialized.StringCollection();
                    if (!Properties.Settings.Default.warnedFMs.Contains(id))
                    {
                        Properties.Settings.Default.warnedFMs.Add(id);
                        Properties.Settings.Default.Save();
                        expiringSoon.Add(post);
                    }

                }
            }

            foreach (MarketPost warn in expiringSoon)
            {
                var user = warn.User;
                string postID = warn.ID;
                int count = warn.Amount;
                string itemName = DBFunctions.GetItemName(warn.Item_ID);
                int price = warn.Price;
                await user.SendMessageAsync("", embed: new InfoEmbed("WARNING: FREE MARKET POSTING EXPIRATION", "This is your one warning that your Free Market posting of " +
                                                                    $"{count} {itemName} for {price} coins will be expiring in 24 hours. You may remove this posting for a 25% fee, or it will " +
                                                                    $"be auctioned off and the coins will go towards slots.").Build());
            }

            bool removed = false;
            List<MarketPost> newBids = new List<MarketPost>();
            foreach (var expiry in expired)
            {
                var user = expiry.User;
                string postID = expiry.ID;
                int count = expiry.Amount;
                string itemName = DBFunctions.GetItemName(expiry.Item_ID);
                int price = expiry.Price;
                await user.SendMessageAsync("", embed: new InfoEmbed("FREE MARKET POSTING EXPIRATION", "This message is to inform you that your Free Market posting of " +
                                                                    $"{count} {itemName} for {price} coins has expired. You have not removed it, and now it will " +
                                                                    $"be auctioned off and the coins will go towards slots.").Build());


                var notifyUsers = DBFunctions.GetUsersWhere("Notify_Bid", "1");

                foreach (IUser u in notifyUsers)
                {
                    await u.SendMessageAsync("", embed: new InfoEmbed("New Bid Alert", $"There is a new bid for {count} {itemName}(s)! Get it with the ID: {postID}.\n*You are receiving this message because you have opted in to new bid notifications.*").Build());
                }
                newBids.Add(expiry);

                MarketPost.DeletePost(postID);
            }

            if (removed)
            {
                foreach (var bid in newBids)
                    new Bid(bid.Item_ID, bid.Amount);
            }

            var bids = Bid.GetAllBids();

            for (int i = 0; i < bids.Count(); i++)
            {
                var endDate = bids[i].EndDate;
                var endTime = endDate - Var.CurrentDate();
                if (endTime <= new TimeSpan(0))
                {
                    var itemID = bids[i].Item_ID;
                    var amount = bids[i].Amount;
                    var bidder = bids[i].CurrentBidder;
                    var currentBid = bids[i].CurrentBid;
                    if (bidder != null)
                    {
                        await bidder.SendMessageAsync($"Congratulations! You've won the bid for {DBFunctions.GetItemEmote(itemID)} {amount} {DBFunctions.GetItemName(itemID)}(s).");
                        var user = Functions.GetUser(bidder);
                        Properties.Settings.Default.jackpot += currentBid;
                        Properties.Settings.Default.Save();
                        for (int j = 0; j < amount; j++) user.GiveItem(itemID);
                    }
                    Bid.DeleteBid(bids[i].ID);
                }
            }

            if ((Var.CurrentDate().DayOfWeek == DayOfWeek.Friday || Var.CurrentDate().DayOfWeek == DayOfWeek.Wednesday) && Properties.Settings.Default.lastBid.DayOfYear != Var.CurrentDate().DayOfYear)
            {
                Properties.Settings.Default.lastBid = Var.CurrentDate();
                Properties.Settings.Default.Save();

                var bidItems = new string[] { "key", "unicorn", "key2", "package", "santa", "gift", "calling" };
                int amount = rdm.Next(10) + 3;
                
                string item = bidItems[rdm.Next(bidItems.Count())];
                var itemID = DBFunctions.GetItemID(item);

                var newBid = new Bid(itemID, amount);

                var notifyUsers = DBFunctions.GetUsersWhere("Notify_Bid", "1");
                foreach (IUser u in notifyUsers)
                {
                    try
                    {
                        await u.SendMessageAsync("", embed: new InfoEmbed("Bi-Weekly Bid Alert", $"The bi-weekly bid is on! This time: {amount} {item}(s)! Get it with the ID: {newBid.ID}.\n*You are receiving this message because you have opted in to new bid notifications.*").Build());
                    }
                    catch (Exception) { } //in case "cannot send message to this user"
                }
            }
        }
    }

    public class MarketPost
    {
        public string ID { get; }
        public IUser User { get; }
        public int Item_ID { get; }
        public int Amount { get; }
        public int Price { get; }
        public DateTime DatePosted { get; }

        public MarketPost(IUser user, int itemID, int amount, int price, DateTime datePosted)
        {
            ID = GenerateID();
            User = user;
            Item_ID = itemID;
            Amount = amount;
            Price = price;
            DatePosted = datePosted;
            Save();
        }

        public MarketPost(string id, IUser user, int itemID, int amount, int price, DateTime datePosted)
        {
            ID = id;
            User = user;
            Item_ID = itemID;
            Amount = amount;
            Price = price;
            DatePosted = datePosted;
        }

        public void Save()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "INSERT INTO FREE_MARKET VALUES(@id, @userid, @itemid, @amount, @price, @date)";
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@id", ID);
                    com.Parameters.AddWithValue("@userid", User.Id);
                    com.Parameters.AddWithValue("@itemid", Item_ID);
                    com.Parameters.AddWithValue("@amount", Amount);
                    com.Parameters.AddWithValue("@price", Price);
                    com.Parameters.AddWithValue("@date", DatePosted);

                    com.ExecuteNonQuery();
                }
            }
        }


        static Random rdm = new Random();
        static string GenerateID()
        {
            string key = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            string id = "";
            do
            {
                id = "";
                for (int i = 0; i < 5; i++)
                {
                    id += key[rdm.Next(key.Count())];
                }
            } while (GetPost(id) != null);

            return id;
        }


        public static MarketPost GetPost(string ID)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT * FROM FREE_MARKET WHERE ID = @id";
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@id", ID);
                    using (var reader = com.ExecuteReader())
                    {
                        if (!reader.HasRows) return null;
                        reader.Read();
                        var user = Bot.client.GetUser((ulong)reader.GetInt64(1));
                        var item_ID = reader.GetInt32(2);
                        var amount = reader.GetInt32(3);
                        var price = reader.GetInt32(4);
                        var datePosted = reader.GetDateTime(5).AddHours(5);

                        return new MarketPost(ID, user, item_ID, amount, price, datePosted);
                    }
                }
            }
        }

        public static MarketPost[] GetAllPosts()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT * FROM FREE_MARKET";
                using (var com = new SQLiteCommand(stm, con))
                {
                    using (var reader = com.ExecuteReader())
                    {
                        List<MarketPost> posts = new List<MarketPost>();
                        while (reader.Read())
                        {
                            var id = reader.GetString(0);
                            var user = Bot.client.GetUser((ulong)reader.GetInt64(1));
                            var item_ID = reader.GetInt32(2);
                            var amount = reader.GetInt32(3);
                            var price = reader.GetInt32(4);
                            var datePosted = reader.GetDateTime(5).AddHours(5);
                            posts.Add(new MarketPost(id, user, item_ID, amount, price, datePosted));
                        }
                        return posts.ToArray();
                    }

                }
            }
        }

        public static MarketPost[] GetPostsByUser(ulong userID)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT * FROM FREE_MARKET WHERE USER_ID = @id";
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@id", userID);
                    using (var reader = com.ExecuteReader())
                    {
                        List<MarketPost> posts = new List<MarketPost>();
                        while (reader.Read())
                        {

                            var post_ID = reader.GetString(0);
                            var user = Bot.client.GetUser((ulong)reader.GetInt64(1));
                            var item_ID = reader.GetInt32(2);
                            var amount = reader.GetInt32(3);
                            var price = reader.GetInt32(4);
                            var datePosted = reader.GetDateTime(5).AddHours(5);

                            posts.Add(new MarketPost(post_ID, user, item_ID, amount, price, datePosted));
                        }
                        return posts.ToArray();
                    }
                }
            }
        }

        public static void DeletePost(string id)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "DELETE FROM FREE_MARKET WHERE ID = @id";
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@id", id);
                    com.ExecuteNonQuery();
                }
            }
        }

    }

    public class Bid
    {
        public string ID { get; }
        public int Item_ID { get; }
        public int Amount { get; }
        public DateTime EndDate { get; }
        public IUser CurrentBidder { get; private set; }
        public int CurrentBid { get; private set; }

        public Bid(int itemID, int amount)
        {
            ID = GenerateID();
            Item_ID = itemID;
            Amount = amount;
            CurrentBid = 100;
            EndDate = Var.CurrentDate().AddHours(24);
            Save();
        }

        public Bid(string id, int itemID, int amount, int currentBid, IUser bidder, DateTime endDate)
        {
            ID = id;
            Item_ID = itemID;
            Amount = amount;
            CurrentBid = currentBid;
            EndDate = endDate;
            CurrentBidder = bidder;
        }

        public void Save()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "INSERT INTO BIDS VALUES(@id, @itemid, @amount, @date, @current_bid, @bidder)";
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@id", ID);
                    com.Parameters.AddWithValue("@itemid", Item_ID);
                    com.Parameters.AddWithValue("@amount", Amount);
                    com.Parameters.AddWithValue("@current_bid", CurrentBid);
                    com.Parameters.AddWithValue("@date", EndDate);
                    if (CurrentBidder != null)
                        com.Parameters.AddWithValue("@bidder", CurrentBidder.Id);
                    else
                        com.Parameters.AddWithValue("@bidder", null);

                    com.ExecuteNonQuery();
                }
            }
        }


        static Random rdm = new Random();
        static string GenerateID()
        {
            string key = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            string id = "";
            do
            {
                id = "";
                for (int i = 0; i < 5; i++)
                {
                    id += key[rdm.Next(key.Count())];
                }
            } while (GetBid(id) != null);

            return id;
        }


        public static Bid GetBid(string ID)
        {
            ID = ID.ToUpper();
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT * FROM BIDS WHERE ID = @id";
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@id", ID);
                    using (var reader = com.ExecuteReader())
                    {
                        if (!reader.HasRows) return null;
                        reader.Read();
                        var itemID = reader.GetInt32(1);
                        var amount = reader.GetInt32(2);
                        var endDate = reader.GetDateTime(3).AddHours(5);
                        var currentBid = reader.GetInt32(4);
                        var bidderID = reader.GetValue(5);
                        
                        IUser currentBidder = null;
                        if (bidderID.GetType() != typeof(DBNull)) 
                            currentBidder = Bot.client.GetUser((ulong)(long)bidderID);

                        return new Bid(ID, itemID, amount, currentBid, currentBidder, endDate);
                    }
                }
            }
        }

        public static Bid[] GetAllBids()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT * FROM BIDS";
                using (var com = new SQLiteCommand(stm, con))
                {
                    using (var reader = com.ExecuteReader())
                    {
                        List<Bid> posts = new List<Bid>();
                        while (reader.Read())
                        {
                            var bidID = reader.GetString(0);
                            var itemID = reader.GetInt32(1);
                            var amount = reader.GetInt32(2);
                            var endDate = reader.GetDateTime(3).AddHours(5);
                            var currentBid = reader.GetInt32(4);
                            var bidderID = reader.GetValue(5);
                            IUser bidder = null;
                            if (bidderID.GetType() != typeof(DBNull))
                                bidder = Bot.client.GetUser((ulong)(long)bidderID);

                            posts.Add(new Bid(bidID, itemID, amount, currentBid, bidder, endDate));
                        }
                        return posts.ToArray();
                    }

                }
            }
        }
        
        public static void DeleteBid(string id)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "DELETE FROM BIDS WHERE ID = @id";
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@id", id);
                    com.ExecuteNonQuery();
                }
            }
        }

        public void Update(IUser newUser, int newBidAmount)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "UPDATE BIDS SET CURRENT_BIDDER_ID = @newUserID, CURRENT_BID = @newBid WHERE ID = @id";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@newUserID", newUser.Id);
                    cmd.Parameters.AddWithValue("@newBid", newBidAmount);
                    cmd.Parameters.AddWithValue("id", ID);

                    cmd.ExecuteNonQuery();
                }
            }

            CurrentBid = newBidAmount;
            CurrentBidder = newUser;
        }
        public void AddTime(TimeSpan time)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "UPDATE BIDS SET END_DATE = @endDate WHERE ID = @id";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@endDate", EndDate.Add(time));
                    cmd.Parameters.AddWithValue("id", ID);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    public class Reminder
    {
        public int ID { get; }
        public IUser User { get; }
        public string Text { get; }
        public DateTime RemindTime { get; }

        public Reminder(IUser user, string reminder, DateTime remindTime)
        {
            User = user;
            Text = reminder;
            RemindTime = remindTime;
            Save();
        }

        public Reminder(int ID, IUser user, string reminder, DateTime remindTime)
        {
            this.ID = ID;
            User = user;
            Text = reminder;
            RemindTime = remindTime;
        }

        //hi bb can u read this i luv u
        public void Save()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "INSERT INTO USER_REMINDERS(USER_ID, TEXT, REMIND_DATE) VALUES(@userID, @text, @remindTime)";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@userID", User.Id);
                    cmd.Parameters.AddWithValue("@text", Text);
                    cmd.Parameters.AddWithValue("@remindTime", RemindTime);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "DELETE FROM USER_REMINDERS WHERE ID = @id";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@id", ID);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        public static Reminder[] GetUserReminders(IUser user)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT * FROM USER_REMINDERS WHERE USER_ID = @userid";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@userid", user.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        List<Reminder> reminders = new List<Reminder>();
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            var text = reader.GetString(2);
                            var date = reader.GetDateTime(3).AddHours(5);
                            reminders.Add(new Reminder(id,user,text,date));
                        }
                        return reminders.ToArray();
                    }
                }
            }
        }

        public static Reminder[] GetAllReminders()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT * FROM USER_REMINDERS";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        List<Reminder> reminders = new List<Reminder>();
                        while (reader.Read())
                        {
                            var id = (ulong)reader.GetInt64(1);
                            var user = Bot.client.GetUser(id);
                            reminders.Add(new Reminder(reader.GetInt32(0),user, reader.GetString(2), reader.GetDateTime(3).AddHours(5)));
                        }
                        return reminders.ToArray();
                    }
                }
            }
        }
    }
}
