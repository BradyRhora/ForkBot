using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForkBot
{
    public class User
    {
        string Username { get; set; }
        int Coins { get; set; }
        long ID { get; set; }

        User(long ID)
        {
            this.ID = ID;
        }

        void Load(string userLine)
        {
            string[] info = userLine.Split('|');
            Username = info[0];
            ID = Convert.ToUInt32(info[1]);
            Coins = Convert.ToInt32(info[2]);
        }
    }
}
