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

        public async Task Run()
        {
            Start:
            try
            {
                DiscordSocketConfig config = new DiscordSocketConfig() { MessageCacheSize = 500 };
                Console.WriteLine("Welcome. Initializing ForkBot...");
                client = new DiscordSocketClient(config);
                Console.WriteLine("Client Initialized.");
                commands = new CommandService();
                Console.WriteLine("Command Service Initialized.");
                await InstallCommands();
                Console.WriteLine("Commands Installed, logging in.");
                await client.LoginAsync(TokenType.Bot, File.ReadAllText("Constants/bottoken"));
                Console.WriteLine("Successfully logged in!");
                await client.StartAsync();
                Console.WriteLine("ForkBot successfully intialized.");
                Functions.LoadUsers();
                Timer banCheck = new Timer(new TimerCallback(TimerCall), null, 0, 1000);
                Timer hourlyTimer = new Timer(new TimerCallback(Hourly), null, 0, 1000 * 60 * 60);
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

        async void TimerCall(object state) //code that is run every second
        {
            for (int i = 0; i < Var.leaveBanned.Count(); i++)
            {
                Console.WriteLine("Attempting to unban..");
                if (DateTime.Now > Var.unbanTime[i])
                {
                    try
                    {
                        var user = Var.leaveBanned[i];
                        var g = user.Guild;
                        await g.RemoveBanAsync(user);
                        Console.WriteLine($"{user} has been unbanned.");
                        Var.leaveBanned.Remove(user);
                        Var.unbanTime.Remove(Var.unbanTime[i]);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unable to unban user. {e.Message}.");
                    }
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
            //client.UserLeft += HandleLeave;
            client.MessageDeleted += HandleDelete;
            client.ReactionAdded += HandleReact;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            
            if (Var.blockedUsers.Contains(message.Author)) return;

            var user = Functions.GetUser(message.Author);
            
            if (Var.recieving)
            {
                if (message.Channel == Var.recievingChannel)
                {
                    var bBunch = client.GetGuild(371695008157532160).GetChannel(381656424247197697) as IMessageChannel;
                    
                    JEmbed emb = new JEmbed();
                    emb.Title = message.Author.Username + "#" + message.Author.Discriminator;
                    emb.Author.Name = "MESSAGE RECIEVED";
                    emb.ThumbnailUrl = message.Author.GetAvatarUrl();
                    emb.Description = message.Content;

                    string attachURL = null;
                    if (message.Attachments.Count > 0) attachURL = message.Attachments.FirstOrDefault().ProxyUrl;
                    if (attachURL != null) emb.ImageUrl = attachURL;

                    await bBunch.SendMessageAsync("", embed: emb.Build());
                }
            }

            if (message.HasCharPrefix(';', ref argPos))
            {
                var context = new CommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos);
                if (!result.IsSuccess)
                {
                    if (result.Error != CommandError.UnknownCommand)
                    {
                        Console.WriteLine(result.ErrorReason);
                        var emb = new InfoEmbed("ERROR:", result.ErrorReason).Build();
                        await message.Channel.SendMessageAsync("", embed: emb);
                    }
                }
            }
            else return;
        }
        public async Task HandleJoin(SocketGuildUser user)
        {
            await (user.Guild.GetChannel(Constants.Channels.GENERAL) as IMessageChannel).SendMessageAsync($"{user.Username}! Welcome to {user.Guild.Name}! Go to <#271843457121779712> to get a role.");
        }
        /*public async Task HandleLeave(SocketGuildUser user)
        {
            await (user.Guild.GetChannel(Constants.Channels.GENERAL) as IMessageChannel).SendMessageAsync($"{user.Username} has left the server.");
            Console.WriteLine($"{user.Username} has been banned for 15 mins due to leaving the server.");
            Var.leaveBanned.Add(user);
            Var.unbanTime.Add(DateTime.Now + new TimeSpan(0, 15, 0));
            await user.Guild.AddBanAsync(user, reason: "Tempban for leaving server. Done automatically by ForkBot to prevent spam leave-joining. To be unbanned at: " + (DateTime.Now + new TimeSpan(0, 15, 0)).TimeOfDay);
        }*/
        public async Task HandleDelete(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {
            var msg = cache.Value;
            
            if ((msg.Author as IGuildUser).Guild.Id == Constants.Guilds.YORK_UNIVERSITY && msg.Author.Id != client.CurrentUser.Id && !Var.purging)
            {
                JEmbed emb = new JEmbed();
                emb.Title = msg.Author.Username + "#" + msg.Author.Discriminator;
                emb.Author.Name = "MESSAGE DELETED";
                emb.ThumbnailUrl = msg.Author.GetAvatarUrl();
                emb.Description = msg.Content;

                string attachURL = null;
                if (msg.Attachments.Count>0) attachURL= msg.Attachments.FirstOrDefault().ProxyUrl;
                if (attachURL != null) emb.ImageUrl = attachURL;

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Location";
                    x.Text = msg.Channel + " in " + (msg.Channel as IGuildChannel).Guild;
                    x.Inline = true;
                }));

                emb.ColorStripe = Constants.Colours.YORK_RED;
                var datetime = DateTime.UtcNow - new TimeSpan(5, 0, 0);
                emb.Footer.Text = datetime.ToLongDateString() + " " + datetime.ToLongTimeString() + " | " +
                    msg.Author.Username + "#" + msg.Author.Discriminator + " ID: " + msg.Author.Id;


                var chan = client.GetChannel(Constants.Channels.DELETED_MESSAGES) as IMessageChannel;
                await chan.SendMessageAsync("", embed: emb.Build());
            }
        }
        public async Task HandleReact(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction react)
        {
            if ((react.UserId != client.CurrentUser.Id))
            {
                string tag = null;
                foreach (IMessage msg in Var.awaitingHelp)
                {
                    if (msg.Id == cache.Value.Id)
                    {
                        if (react.Emote.Name == Constants.Emotes.hammer.Name) tag = "[MOD]";
                        else if (react.Emote.Name == Constants.Emotes.die.Name) tag = "[FUN]";
                        else if (react.Emote.Name == Constants.Emotes.question.Name) tag = "[OTHER]";
                        Var.awaitingHelp.Remove(msg);
                        await msg.DeleteAsync();
                        break;
                    }
                }

                if (tag != null)
                {
                    JEmbed emb = new JEmbed();

                    emb.Author.Name = "ForkBot Commands";
                    emb.ColorStripe = Constants.Colours.DEFAULT_COLOUR;

                    foreach (CommandInfo c in commands.Commands)
                    {
                        string cTag = null;
                        if (c.Summary != null)
                        {
                            if (c.Summary.StartsWith("["))
                            {
                                int index;
                                index = c.Summary.IndexOf(']') + 1;
                                cTag = c.Summary.Substring(0, index);
                            }
                            else cTag = "[OTHER]";
                        }


                        if (cTag != null && cTag == tag)
                        {
                            emb.Fields.Add(new JEmbedField(x =>
                            {
                                string header = c.Name;
                                foreach (String alias in c.Aliases) if (alias != c.Name) header += " (;" + alias + ") ";
                                foreach (Discord.Commands.ParameterInfo parameter in c.Parameters) header += " [" + parameter.Name + "]";
                                x.Header = header;
                                x.Text = c.Summary.Replace(tag + " ","");
                            }));
                        }
                    }
                    await channel.SendMessageAsync("", embed: emb.Build());
                }

            }
        }
    }
}

