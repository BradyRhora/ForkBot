using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    public class Shop
    {
        public List<string> items;
        public List<int> stock;
        DateTime date;
        Random rdm = new Random();

        /*public Shop(List<string> items)
        {
            this.items = items;
            date = DateTime.UtcNow - new TimeSpan(5, 0, 0);
        }*/

        public Shop()
        {
            var nItems = Functions.GetItemList();

            List<string> items = new List<string>();
            List<int> stock = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                int itemID = rdm.Next(nItems.Length);
                if (!items.Contains(nItems[itemID]) && !nItems[itemID].Split('|')[2].Contains("-"))
                {
                    items.Add(nItems[itemID]);
                    stock.Add(rdm.Next(5, 16));
                }
                else i--;
            }

            this.items = items;
            
            date = DateTime.UtcNow - new TimeSpan(5, 0, 0);
        }

        public DateTime Date() { return date; }
        public JEmbed Build()
        {
            JEmbed emb = new JEmbed();
            emb.Title = "Shop";
            emb.ThumbnailUrl = Constants.Images.ForkBot;
            emb.ColorStripe = Constants.Colours.YORK_RED;
            for(int i = 0; i < 5; i++)
            {
                var data = items[i].Split('|');
                string emote = Functions.GetItemEmote(items[i]);
                string name = data[0];
                string desc = data[1];
                int stockAmt = stock[i];
                int price = Convert.ToInt32(data[2]);
                if (price < 0) price = -price;
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = $"{emote} {name.Replace("_", " ")} - {price} coins [{stockAmt} left in stock]";
                    x.Text = desc;
                }));
            }
            return emb;
        }
    }
}
