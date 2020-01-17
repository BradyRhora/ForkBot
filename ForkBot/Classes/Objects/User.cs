using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.IO;
using System.Data.SQLite;

namespace ForkBot
{
    public class User
    {
        public ulong ID { get; }

        public User(ulong ID = 0)
        {
            this.ID = ID;
        }

        public async Task<string> GetName(IGuild guild)
        {
            try
            {
                return Functions.GetName(await guild.GetUserAsync(ID));
            }
            catch(Exception)
            {
                try
                {
                    var user = Bot.client.GetUser(ID);
                    return user.Username;
                }
                catch (Exception) { return null; }
            }
        }

        public void SetData(string data, object value)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();

                var stm = $"UPDATE USERS SET {data} = @value WHERE USER_ID = @userid";

                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@value", value);
                    com.Parameters.AddWithValue("@userid", ID);
                    var obj = com.ExecuteNonQuery();
                }
            }

        }

        public void AddData(string data, int addition)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();

                var stm = $"UPDATE USERS SET {data} = {data}+@addition WHERE USER_ID = @userid";

                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@addition", addition);
                    com.Parameters.AddWithValue("@userid", ID);
                    var obj = com.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        public T GetData<T>(string data)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();

                string stm = $"SELECT {data} FROM USERS WHERE USER_ID = @userid";

                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@userid", ID);
                    var obj = com.ExecuteScalar();
                    var value = (T)Convert.ChangeType(obj, typeof(T));
                    return value;
                }
            }
        }

        public bool GiveItem(string item)
        {
            var itemID = DBFunctions.GetItemID(item);
            if (itemID == 0) return false;
            var hasItem = HasItem(item);

            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();

                string stm = "";
                if (!hasItem)
                    stm = "INSERT INTO USER_ITEMS(USER_ID, ITEM_ID, COUNT) VALUES(@userid, @itemid, 1)";
                else
                    stm = "UPDATE USER_ITEMS SET COUNT = COUNT + 1 WHERE ITEM_ID = @itemid AND USER_ID = @userid";
                
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@userid", ID);
                    com.Parameters.AddWithValue("@itemid", itemID);
                    com.ExecuteNonQuery();
                }
            }
            return true;
        }
        public bool GiveItem(int itemID) { return GiveItem(DBFunctions.GetItemName(itemID)); }
        public void RemoveItem(string item)
        {
            var itemID = DBFunctions.GetItemID(item);
            var hasItem = DBFunctions.UserHasItem(this, itemID);
            if (!hasItem) return;

            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();

                
                var stm = "UPDATE USER_ITEMS SET COUNT = COUNT - 1 WHERE ITEM_ID = @itemid AND USER_ID = @userid";
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@itemid", itemID);
                    com.Parameters.AddWithValue("@userid", ID);
                    com.ExecuteNonQuery();
                }

                var count = DBFunctions.UserItemCount(this, itemID);
                if (count <= 0)
                {
                    stm = "DELETE FROM USER_ITEMS WHERE ITEM_ID = @itemid AND USER_ID = @userid";
                    using (var com = new SQLiteCommand(stm, con))
                    {
                        com.Parameters.AddWithValue("@itemid", itemID);
                        com.Parameters.AddWithValue("@userid", ID);
                        com.ExecuteNonQuery();
                    }
                }

                
            }
        }
        public void RemoveItem(int itemID) => RemoveItem(DBFunctions.GetItemName(itemID));
        public Dictionary<int, int> GetItemList()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();

                string stm = $"SELECT ITEM_ID, COUNT FROM USER_ITEMS WHERE USER_ID = @userid ORDER BY ITEM_ID";

                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@userid", ID);
                    Dictionary<int, int> items = new Dictionary<int, int>();
                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(reader.GetInt32(0), reader.GetInt32(1));
                        }
                        return items;
                    }
                }
            }
        }

        public bool HasItem(int itemID, int amount = 1)
        {
            return DBFunctions.UserHasItem(this, itemID, amount);
        }

        public bool HasItem(string item, int amount = 1)
        {
            return HasItem(DBFunctions.GetItemID(item), amount);
        }

        public void GiveCoins(int amount)
        {

            var topUserStm = $"SELECT USER_ID, COINS FROM USERS ORDER BY COINS DESC LIMIT 1";
            ulong topUserID;
            int topUserAmount;
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();

                using (var com = new SQLiteCommand(topUserStm, con))
                {
                    using (var reader = com.ExecuteReader())
                    {
                        reader.Read();
                        topUserID = (ulong)reader.GetInt64(0);
                        topUserAmount = reader.GetInt32(1);
                    }
                }
                con.Close();
            }
            AddData("coins", amount);
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                ulong newtopUserID;
                using (var com = new SQLiteCommand(topUserStm, con))
                {
                    newtopUserID = (ulong)(long)com.ExecuteScalar();
                }

                if (topUserID != newtopUserID)
                {
                    var name1 = Bot.client.GetUser(topUserID).Username;
                    var name2 = Bot.client.GetUser(newtopUserID).Username;
                    var headline = $"{name2.ToUpper()} TAKES {name1.ToUpper()}'S PLACE AS THE RICHEST!";
                    var content = $"{name2}'s net worth has finally increased beyond {name1}! On {Var.CurrentDate().ToString("dddd, MMMM dd")} at {Var.CurrentDate().ToString("h:mm tt")}," +
                        $" {name2} gained {amount} coins, and it was just enough to put them over {name1}'s current {topUserAmount}. If you need a loan, then" +
                        $" it looks like {name2} is the person to get it from!";
                    if (amount < 0)
                    {
                        headline = $"{name1.ToUpper()}'S NET WORTH PLUMMETS BELOW {name2.ToUpper()}!";
                        content = $"{name1}'s total coins has just gone down past {name2}'s! On {Var.CurrentDate().ToString("dddd, MMMM dd")} at {Var.CurrentDate().ToString("h:mm tt")}," +
                                $" {name1} lost {Math.Abs(amount)} coins, and it was just enough to put them under {name2}. If you need a loan, then" +
                                $" it looks like {name2} is the person to call!";
                    }
                    DBFunctions.AddNews(headline, content);
                }
            }
        }

        public int GetCoins() { return GetData<int>("coins"); }

        public Dictionary<string,int> GetStats()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();

                string stm = $"SELECT * FROM USER_STATS WHERE USER_ID = @userid";

                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@userid", ID);
                    Dictionary<string, int> stats = new Dictionary<string, int>();

                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 1; i < reader.FieldCount; i++)
                            {
                                stats.Add(reader.GetName(i), reader.GetInt32(i));
                            }
                        }
                        return stats;
                    }
                }
            }
        }

        public void AddStat(string stat, int addition)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();

                var topUserStm = $"SELECT USER_ID, {stat} FROM USER_STATS ORDER BY {stat} DESC LIMIT 1";

                ulong topUserID;
                int topUserAmount;
                using (var com = new SQLiteCommand(topUserStm, con))
                {
                    using (var reader = com.ExecuteReader())
                    {
                        reader.Read();
                        topUserID = (ulong)reader.GetInt64(0);
                        topUserAmount = reader.GetInt32(1);
                    }
                }

                var stm = $"UPDATE USER_STATS SET {stat} = {stat}+@addition WHERE USER_ID = @userid";

                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@addition", addition);
                    com.Parameters.AddWithValue("@userid", ID);
                    var obj = com.ExecuteNonQuery();
                }

                ulong newtopUserID;
                using (var com = new SQLiteCommand(topUserStm, con))
                {
                    newtopUserID = (ulong)(long)com.ExecuteScalar();
                }

                if (topUserID != newtopUserID)
                {
                    var name1 = Functions.GetUser(topUserID).GetName(null);
                    var name2 = Functions.GetUser(newtopUserID).GetName(null);
                    var headline = $"{name2} TAKES {name1}'S PLACE AS THE KING OF {stat.ToUpper()}!";
                    var content = $"Outstandingly, {name2} has taken over as the new leader of {stat}! They smashed the current record of {topUserAmount} with ease " +
                        $"on {Var.CurrentDate().ToString("dddd, MMMM dd")} at {Var.CurrentDate().ToString("h:mm tt")}. If you were looking to be the person with the most {stat}," +
                        $" then {name2} is your new rival!";
                    DBFunctions.AddNews(headline, content);
                }
            }
        }

        /*public string[] GetStats()
        {
            return File.ReadAllLines($@"Users\{ID}.user").Where(x => x.StartsWith("stat.")).ToArray();
        }*/
    }
}
