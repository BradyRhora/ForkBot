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
                await client.LoginAsync(TokenType.Bot, File.ReadAllText("Constants/bottoken")); //actual token
                //await client.LoginAsync(TokenType.Bot, "NDMzMzc2MjYxNTU0MDQ0OTM5.Da68oA.5s6xqDZtdO9rkVQlomi0nPQBSg0"); //forkbot test token
                Console.WriteLine("Successfully logged in!");
                await client.StartAsync();
                Console.WriteLine("ForkBot successfully intialized.");
                
                //Timer weeklyTimer = new Timer(new TimerCallback(Weekly), null, 0, 1000 * 60 * 60 * 24 * 7);

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
        
        
        /*void Weekly(object state) //code that is run every week
        {
            //user file purge
            string path = "Files/users.txt";
            var userdata = File.ReadAllLines(path);
            List<String> data = userdata.ToList();
            for (int i = data.Count() - 1; i >= 0; i--)
            {
                if (data[i].Split('|')[1] == "0" && data[i].Split('|').Count() <= 2)
                {
                    data.Remove(data[i]);
                }
            }
            File.WriteAllLines(path, data.ToArray());
        }*/

        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            client.UserJoined += HandleJoin;
            client.UserLeft += HandleLeave;
            client.MessageDeleted += HandleDelete;
            client.ReactionAdded += HandleReact;
            client.MessageUpdated += HandleEdit;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.Id == client.CurrentUser.Id) return; //doesn't allow the bot to respond to itself
            int argPos = 0;
            
            if (Var.blockedUsers.Contains(message.Author)) return; //prevents "blocked" users from using the bot
            
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

            var user = Functions.GetUser(message.Author);
            if (Var.presentWaiting && message.Content == Convert.ToString(Var.presentNum))
            {
                Var.presentWaiting = false;
                await message.Channel.SendMessageAsync($"{message.Author.Username}! You got...");
                var presents = Functions.GetItemList();
                var presentData = presents[rdm.Next(presents.Count())].Split('|');
                Var.present = presentData[0];
                Var.rPresent = Var.present;
                var presentName = Var.present.Replace('_', ' ');
                var pMessage = presentData[1];
                await message.Channel.SendMessageAsync($"A {Func.ToTitleCase(presentName)}! :{Var.present}: {pMessage}");
                if (Var.present == "santa")
                {
                    await message.Channel.SendMessageAsync("You got...");
                    string sMessage = "";
                    for (int i = 0; i < 5; i++)
                    {
                        string sPresent = presents[rdm.Next(presents.Count())].Split('|')[0];
                        sMessage += ":" + sPresent + ": ";
                        user.GiveItem(sPresent);
                    }
                    await message.Channel.SendMessageAsync(sMessage);

                    Var.replaceable = false;
                }
                else user.GiveItem(Var.present);

                if (Var.replaceable)
                {
                    await message.Channel.SendMessageAsync($"Don't like this gift? Press {Var.presentNum} again to replace it once!");
                    Var.replacing = true;
                    Var.presentReplacer = message.Author;
                }
            }
            else if (Var.replaceable && Var.replacing && message.Content == Convert.ToString(Var.presentNum) && message.Author == Var.presentReplacer)
            {
                user.RemoveItem(Var.present);
                await message.Channel.SendMessageAsync("Okay! I'll be right back.");
                await Functions.SendAnimation(message.Channel, Constants.EmoteAnimations.presentReturn, $":{Var.rPresent}:");
                await message.Channel.SendMessageAsync($"A **new** present appears! :gift: Press {Var.presentNum} to open it!");
                Var.presentWaiting = true;
                Var.replacing = false;
                Var.replaceable = false;
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
            else if (message.MentionedUsers.First().Id == client.CurrentUser.Id)
            {
                Functions.Respond(message);
            }
            else return;
        }
        public async Task HandleJoin(SocketGuildUser user)
        {
            await (user.Guild.GetChannel(Constants.Channels.GENERAL) as IMessageChannel).SendMessageAsync($"{user.Username}! Welcome to {user.Guild.Name}! Go to <#271843457121779712> to get a role.");
        }
        public async Task HandleLeave(SocketGuildUser user)
        {
            await (user.Guild.GetChannel(Constants.Channels.GENERAL) as IMessageChannel).SendMessageAsync($"{user.Username} has left the server.");
        }
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
        public async Task HandleEdit(Cacheable<IMessage, ulong> cache, SocketMessage msg, ISocketMessageChannel channel)
        {
            if (msg.Content == cache.Value.Content) return;

            if ((msg.Author as IGuildUser).Guild.Id == Constants.Guilds.YORK_UNIVERSITY && msg.Author.Id != client.CurrentUser.Id && !Var.purging)
            {
                JEmbed emb = new JEmbed();
                emb.Title = msg.Author.Username + "#" + msg.Author.Discriminator;
                emb.Author.Name = "MESSAGE EDITED";
                emb.ThumbnailUrl = msg.Author.GetAvatarUrl();
                
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "ORIGINAL:";
                    x.Text = cache.Value.Content;
                    x.Inline = true;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "EDITED:";
                    x.Text = msg.Content;
                    x.Inline = true;
                }));

                string attachURL = null;
                if (msg.Attachments.Count > 0) attachURL = msg.Attachments.FirstOrDefault().ProxyUrl;
                if (attachURL != null) emb.ImageUrl = attachURL;

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Location";
                    x.Text = msg.Channel + " in " + (msg.Channel as IGuildChannel).Guild;
                    x.Inline = false;
                }));

                emb.ColorStripe = Constants.Colours.TWITTER_BLUE;
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

