using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Discord;

namespace ForkBot
{
    static class DBFunctions
    {

        public static bool UserHasItem(User user, int itemID, int itemCount = 1)
        {
            if (itemID == -1) return false;
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT COUNT(USER_ID), COUNT FROM USER_ITEMS WHERE USER_ID = @userid AND ITEM_ID = @itemid";
                using (var com = new SQLiteCommand(stm, con))
                {

                    com.Parameters.AddWithValue("@userid", user.ID);
                    com.Parameters.AddWithValue("@itemid", itemID);

                    using (var reader = com.ExecuteReader())
                    {
                        reader.Read();
                        bool hasItem = reader.GetInt32(0) != 0;
                        return hasItem && reader.GetInt32(1) >= itemCount;
                    }
                }
            }
        }

        public static bool UserExists(ulong id)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT COUNT(*) FROM USERS WHERE USER_ID = @id";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    var val = cmd.ExecuteScalar();
                    var exists = Convert.ToInt32(val) == 1;
                    return exists;
                }
            }
        }

        public static int UserItemCount(User user, int itemID)
        {
            int itemCount = -1;
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT COUNT FROM USER_ITEMS WHERE USER_ID = @userid AND ITEM_ID = @itemid";
                using (var com = new SQLiteCommand(stm, con))
                {

                    com.Parameters.AddWithValue("@userid", user.ID);
                    com.Parameters.AddWithValue("@itemid", itemID);

                    var value = com.ExecuteScalar();

                    itemCount = Convert.ToInt32(value);
                    
                }
            }
            return itemCount;
        }

        public static IUser[] GetUsersWhere(string data, string value)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = $"SELECT USER_ID FROM USERS WHERE {data} = @value";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@value", value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        List<IUser> users = new List<IUser>();
                        while (reader.Read())
                        {
                            var user = Bot.client.GetUser((ulong)reader.GetInt64(0));
                            if (user != null) users.Add(user);
                        }
                        return users.ToArray();
                    }
                }
            }
        }

        public static int GetItemID(string item)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                item = item.ToLower();
                var getItemID = "SELECT ID FROM ITEMS WHERE ITEM_NAME like @itemN OR EMOTE_NAME like @item";
                using (var com = new SQLiteCommand(getItemID, con))
                {
                    com.Parameters.AddWithValue("@itemN", item.Replace("_"," "));
                    com.Parameters.AddWithValue("@item", item);
                    var value = com.ExecuteScalar();
                    if (value == null) return -1;
                    return Convert.ToInt32(value);
                    
                }
            }
        }

        public static string GetItemName(int itemID)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var getItemID = "SELECT Item_Name FROM ITEMS WHERE ID = @id";
                using (var com = new SQLiteCommand(getItemID, con))
                {
                    com.Parameters.AddWithValue("@id", itemID);
                    var value = com.ExecuteScalar();

                    return value.ToString();

                }
            }
        }

        public static int[] GetItemIDList(bool shoppable = false, bool includeBM = true)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                string stm = $"SELECT ID FROM ITEMS";
                if (!includeBM || shoppable) stm += " WHERE ";
                if (!includeBM) stm += "IS_BLACK_MARKET = false";
                if (shoppable)
                {
                    if (!includeBM) stm += " AND ";
                    stm += "SHOPPABLE = true";
                }

                using (var com = new SQLiteCommand(stm, con))
                {
                    using (var reader = com.ExecuteReader())
                    {
                        List<int> ids = new List<int>();
                        while (reader.Read())
                        {
                            ids.Add(reader.GetInt32(0));
                        }
                        return ids.ToArray();
                    }
                }
            }
        }

        public static int[] GetBMItemIDList()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                string stm = $"SELECT ID FROM ITEMS WHERE Is_Black_Market = true";

                using (var com = new SQLiteCommand(stm, con))
                {
                    using (var reader = com.ExecuteReader())
                    {
                        List<int> ids = new List<int>();
                        while (reader.Read())
                        {
                            ids.Add(reader.GetInt32(0));
                        }
                        return ids.ToArray();
                    }
                }
            }
        }

        public static string[] GetItemNameList(bool includeBM = true)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                string stm = $"SELECT Item_Name FROM ITEMS";
                if (!includeBM) stm += " WHERE IS_BLACK_MARKET = false";

                using (var com = new SQLiteCommand(stm, con))
                {
                    using (var reader = com.ExecuteReader())
                    {
                        List<string> items = new List<string>();
                        while (reader.Read())
                        {
                            items.Add(reader.GetString(0));
                        }
                        return items.ToArray();
                    }
                }
            }
        }

        public static int GetItemPrice(int itemID)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var getItemID = "SELECT Price FROM ITEMS WHERE ID = @id";
                using (var com = new SQLiteCommand(getItemID, con))
                {
                    com.Parameters.AddWithValue("@id", itemID);
                    var value = (int)com.ExecuteScalar();

                    return value;

                }
            }
        }

        public static int GetItemPrice(string item)
        {
            return GetItemPrice(GetItemID(item));
        }

        public static bool ItemIsPresentable(int itemID)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var getItemID = "SELECT Presentable FROM ITEMS WHERE ID = @id";
                using (var com = new SQLiteCommand(getItemID, con))
                {
                    com.Parameters.AddWithValue("@id", itemID);
                    var value = (bool)com.ExecuteScalar();
                    return value;

                }
            }
        }
        public static bool ItemIsShoppable(int itemID)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var getItemID = "SELECT Shoppable FROM ITEMS WHERE ID = @id";
                using (var com = new SQLiteCommand(getItemID, con))
                {
                    com.Parameters.AddWithValue("@id", itemID);
                    var value = (bool)com.ExecuteScalar();

                    return value;

                }
            }
        }
        public static bool ItemIsBM(int itemID)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var getItemID = "SELECT Is_Black_Market FROM ITEMS WHERE ID = @id";
                using (var com = new SQLiteCommand(getItemID, con))
                {
                    com.Parameters.AddWithValue("@id", itemID);
                    var value = (bool)com.ExecuteScalar();

                    return value;

                }
            }
        }

        public static string GetItemEmote(int itemID)
        {
            try
            {
                using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
                {
                    con.Open();
                    var getItemID = "SELECT Emote_Name, Has_Custom_Emote, Emote_ID FROM ITEMS WHERE ID = @id";
                    using (var com = new SQLiteCommand(getItemID, con))
                    {
                        com.Parameters.AddWithValue("@id", itemID);
                        using (var reader = com.ExecuteReader())
                        {
                            string emoteName = "";
                            bool isCustom = false;
                            long id = 0;
                            while (reader.Read())
                            {
                                emoteName = reader.GetString(0);
                                isCustom = reader.GetBoolean(1);
                                id = 0;
                                if (isCustom)
                                {
                                    id = reader.GetInt64(2);
                                }
                            }
                            if (isCustom) return $"<:{emoteName}:{id}>";
                            else return $":{emoteName}:";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return ":question:";
            }
        }
        public static string GetItemEmote(string item)
        {
            return GetItemEmote(GetItemID(item));
        }
        public static string GetItemDescription(int itemID, bool bm = false)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                string type = "Description";
                if (bm) type = "Black_Market_Description";
                var getItemID = $"SELECT {type} FROM ITEMS WHERE ID = @id";
                using (var com = new SQLiteCommand(getItemID, con))
                {
                    com.Parameters.AddWithValue("@id", itemID);

                    var val = com.ExecuteScalar();
                    return val.ToString();
                }
            }
        }

        public static bool StatExists(string stat)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "PRAGMA table_info(USER_STATS)";
                using (var com = new SQLiteCommand(stm, con))
                {
                    using (var reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                            if (reader.GetString(1).ToLower() == stat.ToLower()) return true;
                        
                        return false;
                    }
                    
                }
            }
        }

        public static void AddNews(string headline, string content)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();

                var countStm = "SELECT COUNT(*) FROM NEWSPAPER WHERE HEADER = @head AND CONTENT = @con";
                using (var com = new SQLiteCommand(countStm, con))
                {
                    com.Parameters.AddWithValue("@head", headline);
                    com.Parameters.AddWithValue("@con", content);
                    var count = Convert.ToInt32(com.ExecuteScalar());
                    if (count > 0) return;
                }


                var stm = "INSERT INTO NEWSPAPER VALUES(@headline, @content, @date)";
                using (var com = new SQLiteCommand(stm, con))
                {
                    com.Parameters.AddWithValue("@headline", headline);
                    com.Parameters.AddWithValue("@content", content);
                    com.Parameters.AddWithValue("@date", Var.CurrentDate());
                    com.ExecuteNonQuery();
                }
            }
        }

        public static int GetRelevantNewsCount()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT COUNT(*) FROM NEWSPAPER WHERE DATE_PUBLISHED > @minDate";
                using (var com = new SQLiteCommand(stm, con))
                {
                    DateTime min = Var.CurrentDate().AddHours(-24);
                    com.Parameters.AddWithValue("@minDate", min);
                    var count = Convert.ToInt32(com.ExecuteScalar());
                    return count;
                }
            }
        }

        public static int GetUserRank(IUser user, string col)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = $"select rank from (select user_id, (select count(*) from users b  where a.{col} <= b.{col}) as rank from users a order by rank) where user_id = @id";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    var rank = cmd.ExecuteScalar();
                    return Convert.ToInt32(rank);
                }
            }
        }

        public static int GetAllCoins()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "select sum(coins) from users";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static int GetInventoryValue(IUser user)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "select a.count, b.price from user_items a join items b on (a.ITEM_ID = b.ID) where a.USER_ID = @id";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        int total = 0;
                        while (reader.Read())
                        {
                            total += reader.GetInt32(0) * (int)(reader.GetInt32(1) * Constants.Values.SELL_VAL);
                        }
                        return total;
                    }
                }
            }
        }

        public static int GetUserItemCount(IUser user)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "select sum(count) from user_items where user_id = @id";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    
                    var val = cmd.ExecuteScalar();
                    if (val.GetType() == typeof(DBNull)) return 0;
                    return Convert.ToInt32(val);
                }
            }
        }

        public static int GetTotalItemCount(string itemName)
        {
            return GetTotalItemCount(GetItemID(itemName));
        }
        public static int GetTotalItemCount(int item = -1)
        {
            string specItem = "";
            if (item != -1)
            {
                specItem = " WHERE ITEM_ID = " + item;
            }
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "select sum(count) from user_items" + specItem;
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    var amt = cmd.ExecuteScalar();
                    if (amt.GetType() == typeof(DBNull)) return 0;
                    else return Convert.ToInt32(amt);
                }
            }
        }

        public static int GetTotalStats()
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT sum(HYGIENE + FASHION + HAPPINESS + FITNESS + FULLNESS + HEALTHINESS + SOBRIETY) FROM USER_STATS";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static int GetUserTotalStats(IUser user)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                var stm = "SELECT HYGIENE + FASHION + HAPPINESS + FITNESS + FULLNESS + HEALTHINESS + SOBRIETY FROM USER_STATS WHERE USER_ID = @id";
                using (var cmd = new SQLiteCommand(stm, con))
                {
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

    }
}
