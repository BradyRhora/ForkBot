using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ForkBot
{
    public class ForkParty
    {
        public IMessageChannel Channel;
        public User[] Players = new User[4];
        public int PlayerCount = 1;
        public bool Started = false;

        public ForkParty(User host, IMessageChannel channel)
        {
            Players[0] = host;
            Channel = channel;
        }

        public void Join(User user)
        {
            Players[PlayerCount] = user;
            PlayerCount++;
        }
        public bool HasPlayer(User user)
        {
            for(int i = 0; i < 4; i++) if (Players[0].ID == user.ID) return true;
            return false;
        }

    }
}
