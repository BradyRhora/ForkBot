using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net;
using System.IO;

namespace ForkBot
{
    public class Bot
    {
        static void Main(string[] args) => new Bot().Run().GetAwaiter().GetResult();

        Random rdm = new Random();

        public static DiscordSocketClient client;
        public static CommandService commands;
        public static List<User> users = new List<User>();

        List<IUser> leaveBanned = new List<IUser>();
        List<DateTime> unbanTime = new List<DateTime>();

        #region HangMan vars
        public static string hmWord;
        public static bool hangman = false;
        public static int hmCount = 0;
        public static List<char> guessedChars = new List<char>();
        public static int hmErrors = 0;
        #endregion

        public static bool presentWaiting = false;

        public async Task Run()
        {
            Start:
            try
            {
                Console.WriteLine("Welcome. Initializing ForkBot...");
                client = new DiscordSocketClient();
                Console.WriteLine("Client Initialized.");
                commands = new CommandService();
                Console.WriteLine("Command Service Initialized.");
                await InstallCommands();
                Console.WriteLine("Commands Installed, logging in.");
                await client.LoginAsync(TokenType.Bot, File.ReadAllText("Constants/bottoken"));
                Console.WriteLine("Successfully logged in!");
                await client.StartAsync();
                Console.WriteLine("ForkBot successfully intialized.");
                Timer banCheck = new Timer(new TimerCallback(TimerCall),null,0,60000);
                Timer hourlyTimer = new Timer(new TimerCallback(Hourly), null, 0, 1000*60*60);
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n==========================================================================");
                Console.WriteLine("                                  ERROR                        ");
                Console.WriteLine("==========================================================================\n");
                Console.WriteLine($"Error occured in {e.Source}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.InnerException);

                Again:

                Console.WriteLine("Would you like to try reconnecting? [Y/N]");
                var input = Console.Read();

                if (input == 121) { Console.Clear(); goto Start; }
                else if (input == 110) Environment.Exit(0);

                Console.WriteLine("Invalid input.");
                goto Again;
            }
        }

        async void TimerCall(object state) //code that is run every minute
        {
            for(int i = 0; i < leaveBanned.Count(); i++)
            {
                if (DateTime.Now > unbanTime[i])
                {
                    var g = client.GetGuild(Constants.Guilds.YORK_UNIVERSITY);
                    var user = leaveBanned[i];
                    await g.RemoveBanAsync(user);
                    leaveBanned.Remove(user);
                    unbanTime.Remove(unbanTime[i]);
                }
            }
        }

        void Hourly(object state) //code that is run every hour
        {
            Functions.SaveUsers();
        }

        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            client.UserJoined += HandleJoin;
            client.UserLeft += HandleLeave;
            client.MessageDeleted += HandleDelete;
            client.Ready += HandleReady;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
        
        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;

            if (presentWaiting && message.Content == "4")
            {
                presentWaiting = false;
                await message.Channel.SendMessageAsync($"{message.Author.Username}! You got...");
                var presents = File.ReadAllLines("Files/presents.txt");
                var presentData = presents[rdm.Next(presents.Count())].Split('|');
                var present = presentData[0];
                var pMessage = presentData[1];
                await message.Channel.SendMessageAsync($"A {present}! :{present}: {pMessage}");
            }

            if (message.HasCharPrefix(';', ref argPos))
            {
                var context = new CommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos);
                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.ErrorReason);
                    await message.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
            else return;
        }
        public async Task HandleJoin(SocketGuildUser user)
        {
            await (user.Guild.GetChannel(Constants.Channels.GENERAL) as IMessageChannel).SendMessageAsync($"{user.Username}! Welcome to {user.Guild.Name}! Go to #commands to get a role.");
        }
        public async Task HandleLeave(SocketGuildUser user)
        {
            await (user.Guild.GetChannel(Constants.Channels.GENERAL) as IMessageChannel).SendMessageAsync($"{user.Username} has left the server.");
            Console.WriteLine($"{user.Username} has been banned for 15 mins due to leaving the server.");
            leaveBanned.Add(user);
            unbanTime.Add(DateTime.Now + new TimeSpan(0, 15, 0));
            await user.Guild.AddBanAsync(user, reason:"Tempban for leaving server. Done automatically by ForkBot. To be unbanned at: " + (DateTime.Now + new TimeSpan(0, 15, 0)).TimeOfDay);
        }
        public async Task HandleDelete(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {
            IMessage msg = await cache.DownloadAsync();
            EmbedBuilder emb = new EmbedBuilder();
            emb.Title = "MESSAGE DELETED";
            emb.Author.Name = msg.Author.Username;
            emb.Author.IconUrl = msg.Author.GetAvatarUrl();
            emb.Description = msg.Content;
            var chan = client.GetChannel(Constants.Channels.DELETED_MESSAGES) as IMessageChannel;
            await chan.SendMessageAsync("", embed: emb.Build());
        }
        public async Task HandleReady()    
        {
            Functions.LoadUsers();
        }

        
    }
}

