using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    public class Shop
    {
        public List<int> items;
        public List<int> stock;
        DateTime openDate;
        Random rdm = new Random();
        bool isBM;
        public Shop(bool bm = false)
        {
            isBM = bm;
            int[] nItems;
            if (isBM) nItems = DBFunctions.GetBMItemIDList();
            else nItems = DBFunctions.GetItemIDList(shoppable: true,includeBM: false);
            List<int> items = new List<int>();
            List<int> stock = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                int itemIndex = rdm.Next(nItems.Length);
                if (!items.Contains(nItems[itemIndex]))
                {
                    items.Add(nItems[itemIndex]);
                    if (!isBM) stock.Add(rdm.Next(5, 16));
                    else stock.Add(rdm.Next(1, 5));
                }
                else i--;
            }

            this.items = items;
            this.stock = stock;

            openDate = Var.CurrentDate();
        }

        public DateTime Date() { return openDate; }
        public JEmbed Build()
        {
            JEmbed emb = new JEmbed();
            if (!isBM) emb.Title = "Shop";
            else emb.Title = ":spy: Black Market :spy:";
            emb.ThumbnailUrl = Constants.Images.ForkBot;
            emb.ColorStripe = Constants.Colours.YORK_RED;
            var restock = new TimeSpan(4, 0, 0).Add(openDate - Var.CurrentDate());
            if (!isBM) emb.Description = $"The shop will restock in {restock.Hours} hours and {restock.Minutes} minutes.";
            else emb.Description = $"Welcome to the Black Market... Buy somethin and get out. We'll restock in {restock.Hours} hours and {restock.Minutes} minutes.";
            for(int i = 0; i < 5; i++)
            {
                var itemID = items[i];
                string emote = DBFunctions.GetItemEmote(items[i]);
                string name = DBFunctions.GetItemName(itemID);
                string desc;
                desc = DBFunctions.GetItemDescription(itemID, isBM);
                int stockAmt = stock[i];
                int price = DBFunctions.GetItemPrice(itemID);
                if (price < 0) price = -price;
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = $"{emote} {name.Replace("_", " ")} - {price} coins [{stockAmt} left in stock]";
                    x.Text = desc;
                }));
            }

            if (!isBM)
            {
                var count = DBFunctions.GetRelevantNewsCount();

                if (count > 0)
                {
                    emb.Fields.Add(new JEmbedField(x =>
                    {
                        var newsPrice = DBFunctions.GetItemPrice("newspaper");
                        x.Header = $"📰 Newspaper -  { newsPrice } [({count}) current article(s)]";
                        x.Text = "The Daily Fork! Get all the now information of what's going on around ForkBot!";
                    }));
                }
            }

            return emb;
        }
    }
}
