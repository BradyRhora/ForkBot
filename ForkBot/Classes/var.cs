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
        public static IMessage purgeMessage;

        #region HangMan
        public static string hmWord;
        public static bool hangman = false;
        public static int hmCount = 0;
        public static List<char> guessedChars = new List<char>();
        public static int hmErrors = 0;
        #endregion

        #region Present
        public static string present;
        public static bool presentWaiting = false;
        public static int presentNum = 0;
        public static bool replacing = false;
        public static IUser presentReplacer = null;
        public static string rPresent;
        public static bool replaceable = true;
        public static bool timerComplete = false;
        public static DateTime presentTime = new DateTime(1, 1, 1);
        public static TimeSpan presentWait = new TimeSpan(0,0,0);
        public static int presentCount;
        public static List<IUser> presentClaims = new List<IUser>();
        public static bool presentRigged = false;
        public static IUser presentRigger;
        #endregion

        public static List<ItemTrade> trades = new List<ItemTrade>();

        public static List<IUser> blockedUsers = new List<IUser>();

        public static List<IMessage> awaitingHelp = new List<IMessage>();

        public static Shop currentShop = null;
        public static Shop blackmarketShop = null;

        public static bool responding = true;
        public static string Conversation = "0";

        public static Poll currentPoll;

        public static Dictionary<ulong, DateTime> lastMessage = new Dictionary<ulong, DateTime>();

        public static string todaysLotto = "0";
        public static DateTime lottoDay = new DateTime(0);

        //gets DateTime in Eastern Standard Time
        public static DateTime CurrentDate() { return DateTime.UtcNow - new TimeSpan(6, 0, 0); }
        public static string CurrentDateFormatted() { return $"On {CurrentDate().ToString("dddd, MMMM dd")} at {CurrentDate().ToString("h:mm tt")}"; }

        public static List<ChannelStats> channelStats = new List<ChannelStats>();
        public static DateTime startTime;

        public static int DebugCode;
        public static bool DebugMode = false;

        public static bool LockDown = false;
        public static bool LockDM = false;

        public static List<MineSweeper> MSGames = new List<MineSweeper>();
        public static List<ForkParty> FPGames = new List<ForkParty>();

        public static string term = "FW";

        public static List<IUser> DebugUsers = new List<IUser>();

        public static List<AwaitingVerification> awaitingVerifications = new List<AwaitingVerification>();

        public static DateTime VoidDate = DateTime.Now;
        public static int IDEnd;

        public static List<MaxwellRequest> maxwellRequests = new List<MaxwellRequest>();

    }

    public class MaxwellRequest
    {
        public IUser User { get; }
        public IUserMessage Message { get; }
        public bool isDM { get; }
        public int MaxwellID { get; }
        public string Request { get; }
        public int ID { get; }

        static Random rdm = new Random();
        public MaxwellRequest(IUser user, IUserMessage message, int maxwell, string request, bool dm = false)
        {
            ID = rdm.Next(1000,10000);
            User = user;
            Message = message;
            isDM = dm;
            MaxwellID = maxwell;
            Request = request;
        }
    }
}
