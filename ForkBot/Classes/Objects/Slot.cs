using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    class Slot
    {
        double value;
        string symbol;
        string category;

        public Slot(string symbol, int value)
        {
            this.symbol = $":{symbol}:";
            this.value = value;
            category = "none";
        }
        public Slot(string symbol, int value, string category)
        {
            this.symbol = $":{symbol}:";
            this.value = value;
            this.category = category;
        }

        public double GetValue() { return value; }
        public string GetCategory() { return category; }
        public override string ToString() { return symbol; }
    }
}