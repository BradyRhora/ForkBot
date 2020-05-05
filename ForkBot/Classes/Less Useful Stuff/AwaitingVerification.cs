using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ForkBot
{
    public class AwaitingVerification
    {
        public IUser User { get; }
        public IRole[] Roles { get; }
        public IMessage Message { get; }

        public AwaitingVerification(IUser user, IMessage msg, IRole[] roles)
        {
            User = user;
            Roles = roles;
            Message = msg;
        }
    }
}
