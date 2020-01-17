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
                            users.Add(Bot.client.GetUser((ulong)reader.GetInt64(0)));
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
                var getItemID = "SELECT ID FROM ITEMS WHERE LOWER(ITEM_NAME) = @item OR EMOTE_NAME = @item";
                using (var com = new SQLiteCommand(getItemID, con))
                {
                    com.Parameters.AddWithValue("@item", item);
                    var value = com.ExecuteScalar();
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

        public static int[] GetItemIDList(bool includeBM = true)
        {
            using (var con = new SQLiteConnection(Constants.Values.DB_CONNECTION_STRING))
            {
                con.Open();
                string stm = $"SELECT ID FROM ITEMS";
                if (!includeBM) stm += " WHERE IS_BLACK_MARKET = false";
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
    }
}
