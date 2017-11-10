using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ForkBot
{
    public class Functions
    {
        public static Color GetColor(IUser User)
        {
            var user = User as IGuildUser;
            if (user == null)
            {
                if (user.RoleIds.ToArray().Count() > 1)
                {
                    var role = Bot.client.GetGuild(Constants.Guilds.YORK_UNIVERSITY).GetRole(user.RoleIds.ElementAtOrDefault(1));
                    return role.Color;
                }
                else return Constants.Colours.DEFAULT_COLOUR;
            }
            else return Constants.Colours.DEFAULT_COLOUR;
        }
    }
}
