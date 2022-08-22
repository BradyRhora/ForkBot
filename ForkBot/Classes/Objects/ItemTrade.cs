using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;

namespace ForkBot
{
    public class ItemTrade
    {
        public User u1, u2;
        List<string> items1, items2;
        int coins1 = 0, coins2 = 0;
        public bool Accepted;
        bool confirmed1, confirmed2;
        bool completed = false;
        public ItemTrade(IUser userOne, IUser userTwo)
        {
            u1 = User.Get(userOne);
            u2 = User.Get(userTwo);
            items1 = new List<string>();
            items2 = new List<string>();
            coins1 = 0;
            coins2 = 0;
            Accepted = false;
            confirmed1 = false;
            confirmed2 = false;
        }

        /// <summary>
        /// Adds the inputted item to the inputted users list. Returns true when successful, false when not, usually when user doesn't have the item.
        /// </summary>
        /// <param name="u">The user adding the item</param>
        /// <param name="item">The item to be added</param>
        /// <returns></returns>
        public async Task<bool> AddItemAsync(IUser u, string item, int amount)
        {
            if (!Accepted) return false;
            

            int coins;
            if (u.Id == u1.ID)
            {
                if (int.TryParse(item, out coins))
                {
                    if (coins > 0 && u1.GetCoins() >= coins)
                    {
                        coins1 += coins;
                        await u1.GiveCoinsAsync(-coins);
                        return true;
                    }
                }

                var itemList = u1.GetItemList();
                var itemID = DBFunctions.GetItemID(item);
                if (!itemList.ContainsKey(itemID)) return false;
                int userHas = itemList[itemID];
                if (amount <= userHas)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        items1.Add(item);
                        u1.RemoveItem(item);
                    }
                    return true;
                }
                else return false;
                
            }
            else
            {
                if (int.TryParse(item, out coins))
                {
                    if (coins > 0 && u2.GetCoins() >= coins)
                    {
                        coins2 += coins;
                        await u2.GiveCoinsAsync(-coins);
                        return true;
                    }
                }

                var itemList = u2.GetItemList();
                var itemID = DBFunctions.GetItemID(item);
                if (!itemList.ContainsKey(itemID)) return false;
                int userHas = itemList[itemID];
                if (amount <= userHas)
                {
                    for (int i = 0; i < amount; i++)
                    {
                        items2.Add(item);
                        u2.RemoveItem(item);
                    }
                    return true;
                }
                else return false;
            }

        }

        /// <summary>
        /// Generates the embed menu to display the trade info.
        /// </summary>
        public async Task<Embed> CreateMenuAsync()
        {
            JEmbed emb = new JEmbed();
            string u1Name = (await Bot.client.GetUserAsync(u1.ID)).Username;
            string u2Name = (await Bot.client.GetUserAsync(u2.ID)).Username;
            emb.Title = $"Trade: {u1Name} - {u2Name}";
            emb.Description = "Use `;trade add [item]` to add an item, or `;trade add [number]` to add coins.\nWhen done, use `;trade finish` or use `;trade cancel` to cancel the trade.\nYou can now put `*[#]` at the end to add multiple items! (i.e. `;trade add key*10`)";

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = u1Name + "'s Items";

                string itemlist = "";
                foreach(string item in items1)  itemlist += DBFunctions.GetItemEmote(item);
                if(coins1 > 0) itemlist += ":moneybag:" + coins1 + " coins";

                x.Text = itemlist;
                x.Inline = true;
            }));
            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = u2Name + "'s Items";

                string itemlist = "";
                foreach (string item in items2) itemlist += DBFunctions.GetItemEmote(item);
                if (coins2 > 0) itemlist += ":moneybag:" + coins2 + " coins";

                x.Text = itemlist;
                x.Inline = true;
            }));

            emb.ColorStripe = Constants.Colours.YORK_RED;
            emb.Author.IconUrl = Constants.Images.ForkBot;
            emb.Author.Name = "Forkbot Trade Menu:tm:";

            return emb.Build();
        }

        public bool HasUser(IUser user)
        {
            if (u1.ID == user.Id || u2.ID == user.Id) return true;
            return false;
        }

        public void Accept()
        {
            Accepted = true;
        }

        public ulong Starter()
        {
            return u1.ID;
        }

        public async Task ConfirmAsync(IUser user)
        {
            if (Starter() == user.Id) confirmed1 = true;
            else confirmed2 = true;

            if (confirmed1 && confirmed2)
            {
                await CompleteTradeAsync();
            }
        }

        async Task CompleteTradeAsync()
        {
            foreach(string item in items1)
            {
                if (item != "heart") u2.GiveItem(item);
                else u2.GiveItem("gift");
            }

            if (coins1 > 0)
            {
                await u2.GiveCoinsAsync(coins1);
            }

            foreach (string item in items2)
            {
                if (item != "heart") u1.GiveItem(item);
                else u1.GiveItem("gift");
            }

            if (coins2 > 0)
            {
                await u1.GiveCoinsAsync(coins2);
            }

            completed = true;
        }

        public bool IsCompleted()
        {
            return completed;
        }

        public async Task CancelAsync()
        {
            Var.trades.Remove(this);
            foreach(string item in items1)
            {
                u1.GiveItem(item);
            }
            await u1.GiveCoinsAsync(coins1);
            foreach (string item in items2)
            {
                u2.GiveItem(item);
            }
            await u2.GiveCoinsAsync(coins2);
        }
    }
}
