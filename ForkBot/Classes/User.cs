using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ForkBot
{
    public class User
    {
        public int Coins { get; set; }
        public ulong ID { get; set; }
        public List<string> Items = new List<string>();

        public User(ulong ID = 0, Boolean load = false, string data = "")
        {
            this.ID = ID;
            if (load) Load(data);
        }

        User Load(string userLine)
        {
            string[] info = userLine.Split('|');
            ID = Convert.ToUInt64(info[0]);
            Coins = Convert.ToInt32(info[1]);
            for (int i = 2; i < info.Count(); i++) Items.Add(info[i]);
            return this;
        }

        public string Username()
        {
            return Bot.client.GetUser(ID).Username;
        }
    }
}
