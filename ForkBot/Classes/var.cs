using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace ForkBot
{
    public class Var
    {
        public static List<IGuildUser> leaveBanned = new List<IGuildUser>();
        public static List<DateTime> unbanTime = new List<DateTime>();

        public static bool purging = false;

        #region HangMan
        public static string hmWord;
        public static bool hangman = false;
        public static int hmCount = 0;
        public static List<char> guessedChars = new List<char>();
        public static int hmErrors = 0;
        #endregion
        
        public static List<ItemTrade> trades = new List<ItemTrade>();

        public static List<IUser> blockedUsers = new List<IUser>();

        public static List<IMessage> awaitingHelp = new List<IMessage>();

        public static Shop currentShop = null;

    }
}
