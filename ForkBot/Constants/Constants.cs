using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace ForkBot
{
    class Constants
    {
        public class Guilds
        {
            public static ulong YORK_UNIVERSITY = 265998661606047744;
        }

        public class Users
        {
            public static ulong BRADY = 108312797162541056;
        }

        public class Roles
        {
            public static ulong ADMIN = 000000000000000;
            public static ulong MOD = 000000000000000;
            public static ulong NON_YORK = 000000000000000;
        }

        public class Colours
        {
            public static Color YORK_RED = new Color(197, 29, 64);
            public static Color DEFAULT_COLOUR = new Color(197, 29, 64);
        }

        public class Channels
        {
            public static ulong GENERAL = 265998661606047744;
            public static ulong MEMES = 266001105350164480;
            public static ulong ENTERTAINMENT = 266001204499316736;
            public static ulong ANNOUNCEMENTS = 265999135503679488;
            public static ulong COMMANDS = 271843457121779712;
            public static ulong DELETED_MESSAGES = 306236074655612930;
        }
    }
}
