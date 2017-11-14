using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    public class User
    {
        public string Username { get; set; }
        public int Coins { get; set; }
        public ulong ID { get; set; }
        
        public User(ulong ID = 0, Boolean load = false, string data = "")
        {
            this.ID = ID;
            if (load) Load(data);
        }

        User Load(string userLine)
        {
            string[] info = userLine.Split('|');
            Username = info[0];
            ID = Convert.ToUInt32(info[1]);
            Coins = Convert.ToInt32(info[2]);
            return this;
        }
    }
}
