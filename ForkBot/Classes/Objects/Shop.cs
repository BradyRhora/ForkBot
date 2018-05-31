using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    public class Shop
    {
        List<string> items;
        DateTime date;
        Random rdm;

        public Shop(List<string> items)
        {
            this.items = items;
            date = DateTime.UtcNow - new TimeSpan(5, 0, 0);
        }

        public Shop()
        {
            var nItems = Functions.GetItemList();
            var rItems = Functions.GetRareItemList();
            var allItems = nItems.Concat(rItems).ToArray();

            List<string> items = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                int itemID = rdm.Next(allItems.Length);
                if (!items.Contains(allItems[itemID]) && !allItems[itemID].Split('|').Contains("-")) items.Add(allItems[itemID]);
                else i--;
            }

            this.items = items;
            date = DateTime.UtcNow - new TimeSpan(5, 0, 0);
        }

        public DateTime Date() { return date; }
        public List<string> Items() { return items; }
    }
}
