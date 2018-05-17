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
            public static ulong P10_ENTERPRISES = 436016366672150531;
        }

        public class Users
        {
            public static ulong BRADY = 108312797162541056;
        }

        public class Roles
        {
            public static ulong ADMIN = 000000000000000;
            public static ulong MOD = 266000331442356225;
            public static ulong NON_YORK = 000000000000000;
        }

        public class Colours
        {
            public static Color YORK_RED = new Color(197, 29, 64);
            public static Color DEFAULT_COLOUR = new Color(197, 29, 64);
            public static Color TWITTER_BLUE = new Color(0, 172, 237);
        }

        public class Channels
        {
            public static ulong GENERAL = 265998661606047744;
            public static ulong GENERAL_2 = 379809861317165058;
            public static ulong MEMES = 266001105350164480;
            public static ulong ENTERTAINMENT = 266001204499316736;
            public static ulong ANNOUNCEMENTS = 265999135503679488;
            public static ulong COMMANDS = 271843457121779712;
            public static ulong DELETED_MESSAGES = 306236074655612930;
        }

        public class EmoteAnimations
        {
            public static EmoteAnimation presentReturn = new EmoteAnimation(new string[] {
                ":black_large_square::black_large_square::black_large_square::convenience_store::black_large_square:%:runner:",
                ":black_large_square::black_large_square::black_large_square::convenience_store:%:runner::black_large_square:",
                ":black_large_square::black_large_square::black_large_square::convenience_store::runner::black_large_square::black_large_square:",
                ":black_large_square::black_large_square::gift::convenience_store::black_large_square::black_large_square::black_large_square:",
                ":black_large_square::gift::runner::convenience_store::black_large_square::black_large_square::black_large_square:",
                ":gift::runner::black_large_square::convenience_store::black_large_square::black_large_square::black_large_square:"});
        }

        public class Images
        {
            public const string Ban = "https://i.imgur.com/S8nPf5Y.png";
            public const string Kick = "https://i.imgur.com/Pid9NH3.png";
            public const string ForkBot = "https://i.imgur.com/xz1OuJr.png";
        }

        public class Emotes
        {

            public static Emoji hammer = new Emoji("🔨");
            public static Emoji die = new Emoji("🎲");
            public static Emoji question = new Emoji("❓");
            public static Emote chad = Emote.Parse("<:CHAD:436784932820353024>");
            

        }



    }
}
