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
            u1 = Functions.GetUser(userOne);
            u2 = Functions.GetUser(userTwo);
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
        public bool AddItem(IUser u, string item, int amount)
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
                        u1.GiveCoins(-coins);
                        return true;
                    }
                }

                var itemList = u1.GetItemList();
                int userHas = itemList.Where(x => x == item).Count();
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
                        u2.GiveCoins(-coins);
                        return true;
                    }
                }

                var itemList = u2.GetItemList();
                int userHas = itemList.Where(x => x == item).Count();
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
        /// <returns></returns>
        public Embed CreateMenu()
        {
            JEmbed emb = new JEmbed();
            string u1Name = Bot.client.GetUser(u1.ID).Username;
            string u2Name = Bot.client.GetUser(u2.ID).Username;
            emb.Title = $"Trade: {u1Name} - {u2Name}";
            emb.Description = "Use `;trade add [item]` to add an item, or `;trade add [number]` to add coins.\nWhen done, use `;trade finish` or use `;trade cancel` to cancel the trade.\nYou can now put `*[#]` at the end to add multiple items! (i.e. `;trade add key*10`)";

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = u1Name + "'s Items";

                string itemlist = "";
                foreach(string item in items1)  itemlist += Functions.GetItemEmote(item);
                if(coins1 > 0) itemlist += ":moneybag:" + coins1 + " coins";

                x.Text = itemlist;
                x.Inline = true;
            }));
            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = u2Name + "'s Items";

                string itemlist = "";
                foreach (string item in items2) itemlist += Functions.GetItemEmote(item);
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

        public IUser Starter()
        {
            return Bot.client.GetUser(u1.ID);
        }

        public void Confirm(IUser user)
        {
            if (Starter().Id == user.Id) confirmed1 = true;
            else confirmed2 = true;

            if (confirmed1 && confirmed2)
            {
                CompleteTrade();
            }
        }

        void CompleteTrade()
        {
            foreach(string item in items1)
            {
                if (item != "heart") u2.GiveItem(item);
                else u2.GiveItem("gift");
            }

            if (coins1 > 0)
            {
                u2.GiveCoins(coins1);
            }

            foreach (string item in items2)
            {
                if (item != "heart") u1.GiveItem(item);
                else u1.GiveItem("gift");
            }

            if (coins2 > 0)
            {
                u1.GiveCoins(coins2);
            }

            completed = true;
        }

        public bool IsCompleted()
        {
            return completed;
        }

        public void Cancel()
        {
            Var.trades.Remove(this);
            foreach(string item in items1)
            {
                u1.GiveItem(item);

            }
            u1.GiveCoins(coins1);
            foreach (string item in items2)
            {
                u2.GiveItem(item);

            }
            u2.GiveCoins(coins2);
        }
    }
}
