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

        public Shop(List<string> items)
        {
            this.items = items;
            date = DateTime.UtcNow - new TimeSpan(5, 0, 0);
        }

        public DateTime Date() { return date; }
        public List<string> Items() { return items; }
    }
}
