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
        Random rdm = new Random();

        public Shop(List<string> items)
        {
            this.items = items;
            date = DateTime.UtcNow - new TimeSpan(5, 0, 0);
        }

        public Shop()
        {
            var nItems = Functions.GetItemList();

            List<string> items = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                int itemID = rdm.Next(nItems.Length);
                if (!items.Contains(nItems[itemID]) && !nItems[itemID].Split('|')[2].Contains("-")) items.Add(nItems[itemID]);
                else i--;
            }

            this.items = items;
            date = DateTime.UtcNow - new TimeSpan(5, 0, 0);
        }

        public DateTime Date() { return date; }
        public List<string> Items() { return items; }
    }
}
