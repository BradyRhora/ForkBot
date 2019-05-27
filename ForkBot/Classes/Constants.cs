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
            public static ulong FORKU = 342836847187329024;
            public static ulong BASSIC = 492147609532891136;
        }

        public class Users
        {
            public static ulong BRADY = 108312797162541056;
            public static ulong DYNO = 155149108183695360;
            public static ulong JACE = 308761161258500097;
            public static ulong FORKPY = 573047938306146304;
        }

        public class Roles
        {
            public static ulong ADMIN = 000000000000000;
            public static ulong MOD = 266000331442356225;
            public static ulong NON_YORK = 000000000000000;
            public static ulong TTS = 369001773202931734;
            public static ulong DUST = 562334977606418433;
            public static ulong TRUSTED = 561299358637752345;
        }

        public class Colours
        {
            public static Color YORK_RED = new Color(197, 29, 64);
            public static Color DEFAULT_COLOUR = new Color(197, 29, 64);
            public static Color TWITTER_BLUE = new Color(0, 172, 237);
        }

        public class Channels
        {
            public static ulong GENERAL_TRUSTED = 561299616746700860;
            public static ulong GENERAL_SLOW = 265998661606047744;
            public static ulong ENTERTAINMENT = 266001204499316736;
            public static ulong ANNOUNCEMENTS = 265999135503679488;
            public static ulong COMMANDS = 271843457121779712;
            public static ulong DELETED_MESSAGES = 306236074655612930;
            public static ulong ASS_DELETED_MESSAGES = 493873769627254786;
            public static ulong REPORTED = 479383993360449544;
            public static ulong NEWS_DEBATE = 364867272126365696;
            public static ulong LIFESTYLE = 404142837685551107;
            public static ulong DEBUG = 582651340157878283;
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

            public static Emoji HAMMER = new Emoji("🔨");
            public static Emoji DIE = new Emoji("🎲");
            public static Emoji QUESTION = new Emoji("❓");
            public static Emote BRADY = Emote.Parse("<:brady:465359176575614980>");
            

        }

        public class Dates
        {
            public static DateTime STRIKE_END = new DateTime(2018, 7, 25);
        }

        public class Values
        {
            public static double SELL_VAL = .45;
            public static string GNOME_VID = "https://tenor.com/view/gnome-your-chums-gnomed-gnome-gnoblin-gnelf-gif-12675740";
        }
    }
}
