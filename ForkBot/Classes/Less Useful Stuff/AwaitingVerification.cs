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

        public AwaitingVerification(IUser user, IRole[] roles)
        {
            User = user;
            Roles = roles;
        }
    }
}
