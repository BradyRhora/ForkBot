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
                DiscordSocketConfig config = new DiscordSocketConfig() { MessageCacheSize = 1000 };
                Console.WriteLine("Welcome. Initializing ForkBot...");
                client = new DiscordSocketClient(config);
                Console.WriteLine("Client Initialized.");
                commands = new CommandService();
                Console.WriteLine("Command Service Initialized.");
                await InstallCommands();
                Console.WriteLine("Commands Installed, logging in.");
                if (!Directory.Exists("Constants"))
                {
                    Directory.CreateDirectory("Constants");
                    Console.WriteLine("Created Constants folder in bin/Debug/");
                }
                if (!File.Exists("Constants/bottoken"))
                {
                    File.WriteAllText("Constants/bottoken", "");
                    Console.WriteLine("Created bottoken file in Constants folder, you will need to put the token in this file.");
                }
                await client.LoginAsync(TokenType.Bot, File.ReadAllText("Constants/bottoken"));
                
                Console.WriteLine("Successfully logged in!");
                await client.StartAsync();
                Var.DebugCode = rdm.Next(999, 9999) + 1;
                Console.WriteLine($"ForkBot successfully intialized with debug code [{Var.DebugCode}]");
                Var.startTime = Var.CurrentDate();
                int strikeCount = (Var.CurrentDate() - Constants.Dates.STRIKE_END).Days;
                await client.SetGameAsync(strikeCount + " days since last strike", type: ActivityType.Watching);
                Timers.RemindTimer = new Timer(Timers.Remind, null, 1000 * 30, 1000 * 60);
                Timers.BidTimer = new Timer(Timers.Bid, null, 1000 * 30, 1000 * 60);
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
        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            client.UserJoined += HandleJoin;
            client.UserLeft += HandleLeave;
            client.MessageDeleted += HandleDelete;
            client.ReactionAdded += HandleReact;
            client.MessageUpdated += HandleEdit;
            client.UserVoiceStateUpdated += HandleVoiceUpdate;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services: null);
        }

        DateTime lastDay = Var.CurrentDate();
        public async Task HandleCommand(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            bool isDM = await Functions.isDM(message as IMessage);
            if (message == null) return;
            if (message.Author.Id == client.CurrentUser.Id) return; //doesn't allow the bot to respond to itself
            if (Var.DebugMode && message.Author.Id != Constants.Users.BRADY && Var.DebugUsers.Where(x=>x.Id==message.Author.Id).Count() <= 0) return;
            if (!Var.DebugMode && message.Channel.Id == Constants.Channels.DEBUG) return;
            var user = Functions.GetUser(message.Author);
            //trusted management
            if (!isDM && (message.Channel as IGuildChannel).Guild.Id == Constants.Guilds.YORK_UNIVERSITY)
            {
                string trustedMsgs = user.GetData("trustedMsgs");
                if (trustedMsgs == "0") user.SetData("trustedMsgs", "false");
                if (trustedMsgs != "true")
                {
                    if (message.Channel.Id == Constants.Channels.GENERAL_SLOW)
                    {
                        int msgCount = Convert.ToInt32(user.GetData("messages")) + 1;

                        if (msgCount >= 500)
                        {
                            user.SetData("trustedMsgs", "true");
                        }
                        else user.SetData("messages", Convert.ToString(msgCount));
                    }
                }



                var guildUser = message.Author as IGuildUser;
                var guild = (message.Channel as IGuildChannel).Guild;
                var reports = await guild.GetChannelAsync(Constants.Channels.REPORTED) as IMessageChannel;
                if (message.Channel.Id == Constants.Channels.REPORTED && message.Author.Id == Constants.Users.DYNO)
                {
                    var emb = message.Embeds.FirstOrDefault();
                    if (emb != null)
                    {
                        ulong id = Convert.ToUInt64(emb.Footer.Value.Text.Replace("ID: ", ""));
                        var u = Functions.GetUser(id);
                        u.SetData("lastInfraction", Functions.DateTimeToString(Var.CurrentDate()));
                        if (u.GetData("isTrusted") == "true")
                        {
                            u.SetData("isTrusted", "false");
                            var gUser = await guild.GetUserAsync(u.ID);
                            await gUser.RemoveRoleAsync(guild.GetRole(Constants.Roles.TRUSTED));
                            string reason = emb.Fields[0].Value;
                            await reports.SendMessageAsync($"Removed Trusted role from {gUser.Mention} for reason:\n```\nAutomod infraction:\n'{reason}'```");
                            await gUser.SendMessageAsync($"Removed Trusted role due to:\n```\nAutomod infraction:\n`{reason}'```");
                        }

                    }
                }

                var lastInfraction = user.GetData("lastInfraction");
                bool threeDaysSinceLast = false;
                if (lastInfraction == "0" || Var.CurrentDate() - Functions.StringToDateTime(user.GetData("lastInfraction")) > new TimeSpan(3, 0, 0, 0)) threeDaysSinceLast = true;

                var trustedStat = user.GetData("isTrusted");
                bool trusted = true;
                if (trustedStat == "false" || trustedStat == "0") trusted = false;

                if (threeDaysSinceLast == false && trusted)
                {
                    await guildUser.RemoveRoleAsync(guild.GetRole(Constants.Roles.TRUSTED));
                    user.SetData("isTrusted", "false");
                    trusted = false;
                    await reports.SendMessageAsync($"Removed Trusted role from {guildUser.Mention} for reason:\n```\nLess than one week since last infraction.\n```");
                }

                if (!trusted && guildUser.RoleIds.ToArray().Contains(Constants.Roles.TRUSTED))
                {
                    await guildUser.RemoveRoleAsync(guild.GetRole(Constants.Roles.TRUSTED));
                    await reports.SendMessageAsync($"Removed Trusted role from {guildUser.Mention} for reason:\n```\nHad role but was not trusted.\n```");
                }

                if (trusted && !guildUser.RoleIds.ToArray().Contains(Constants.Roles.TRUSTED))
                {
                    await guildUser.AddRoleAsync(guild.GetRole(Constants.Roles.TRUSTED));
                    await reports.SendMessageAsync($"Added Trusted role to {guildUser.Mention} for reason:\n```\nWas trusted but did not have role.\n```");
                }


                if (!trusted && Var.CurrentDate() - (guildUser.JoinedAt - new TimeSpan(4,0,0)) >= new TimeSpan(3, 0, 0, 0) && user.GetData("trustedMsgs") == "true" && threeDaysSinceLast)
                {
                    user.SetData("isTrusted", "true");
                    await guildUser.AddRoleAsync(guild.GetRole(Constants.Roles.TRUSTED));
                    await reports.SendMessageAsync($"Added Trusted role to {guildUser.Mention} for reason:\n```\nFulfilled all requirements.\n```");
                    await guildUser.SendMessageAsync("You have successfully fulfilled all requirements and have gained the trusted role. You now have access to all `trusted` channels.");
                }
            }

            
            int argPos = 0;

            if (lastDay.DayOfYear < Var.CurrentDate().DayOfYear)
            {
                int strikeCount = (Var.CurrentDate() - Constants.Dates.STRIKE_END).Days;
                await client.SetGameAsync(strikeCount + " days since last strike", type: ActivityType.Watching);
            }

            //checks if message contains any blocked words
            if (!isDM && (message.Channel as IGuildChannel).Guild.Id == Constants.Guilds.YORK_UNIVERSITY && Functions.Filter(message.Content))
            {
                await message.DeleteAsync();
                return;
            }

            if (Var.blockedUsers.Where(x=>x.Id == message.Author.Id).Count() > 0) return; //prevents "blocked" users from using the bot

            ulong[] blockedChannels = { Constants.Channels.GENERAL_SLOW, Constants.Channels.GENERAL_TRUSTED, Constants.Channels.NEWS_DEBATE, Constants.Channels.LIFESTYLE };
            if (!isDM && (message.Channel as IGuildChannel).Guild.Id == Constants.Guilds.YORK_UNIVERSITY && (blockedChannels.Contains(message.Channel.Id)) && !(message.Author as IGuildUser).RoleIds.Contains(Constants.Roles.MOD)) return;
            
            if (message.Author.IsBot && message.Author.Id != Constants.Users.FORKPY) return;
            
            //present stuff
            if (Var.presentWaiting && message.Content == Convert.ToString(Var.presentNum))
            {
                Var.presentWaiting = false;
                await message.Channel.SendMessageAsync($"{message.Author.Username}! You got...");
                var presents = Functions.GetItemList();
                int presRDM;
                string[] presentData;
                do
                {
                    presRDM = rdm.Next(presents.Count());
                    presentData = presents[presRDM].Split('|');
                } while (presentData[2].Contains("*"));
                Var.present = presentData[0];
                Var.rPresent = Var.present;
                var presentName = Var.present;
                var pMessage = presentData[1];
                await message.Channel.SendMessageAsync($"A {Func.ToTitleCase(presentName.Replace('_', ' '))}! {Functions.GetItemEmote(presents[presRDM])} {pMessage}");
                if (Var.present == "santa")
                {
                    await message.Channel.SendMessageAsync("You got...");
                    string sMessage = "";
                    for (int i = 0; i < 5; i++)
                    {
                        var sPresentData = presents[rdm.Next(presents.Count())];
                        if (sPresentData.Contains("*"))
                        {
                            i--;
                            continue;
                        }
                        string sPresentName = sPresentData.Split('|')[0];
                        user.GiveItem(sPresentName);
                        sMessage += $"A {Func.ToTitleCase(sPresentName)}! {Functions.GetItemEmote(sPresentData)} {sPresentData.Split('|')[1]}\n";
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
                if (user.GetItemList().Contains(Var.present))
                {
                    user.RemoveItem(Var.present);
                    await message.Channel.SendMessageAsync($":convenience_store: {Functions.GetItemEmote(Var.present)} :runner: \nA **new** present appears! :gift: Press {Var.presentNum} to open it!");
                    Var.presentWaiting = true;
                    Var.replacing = false;
                    Var.replaceable = false;
                }
                else
                {
                    await message.Channel.SendMessageAsync("You no longer have the present, so you cannot replace it!");
                    Var.replacing = false;
                    Var.replaceable = false;
                }
            }

            //detects invites for unwanted servers (in yorku server) and deletes them
            if (!isDM && (message.Channel as IGuildChannel).Guild.Id == Constants.Guilds.YORK_UNIVERSITY && message.Content.ToLower().Contains("discord.gg") || message.Content.ToLower().Contains("discordapp.com/invite"))
            {
                var words = message.Content.Split(' ');
                foreach (string word in words)
                {
                    if (word.Contains("discord"))
                    {
                        string id = word.Split('/')[word.Split('/').Count() - 1];
                        IInvite inv = await client.GetInviteAsync(id);
                        if (inv.GuildId == Constants.Guilds.FORKU)
                        {
                            await messageParam.DeleteAsync();
                        }
                        return;
                    }
                }
            }

            //detect and execute commands
            if (message.HasCharPrefix(';', ref argPos))
            {
                var context = new CommandContext(client, message);
                var result = await commands.ExecuteAsync(context, argPos, services: null);

                if (!result.IsSuccess)
                {
                    if (result.Error != CommandError.UnknownCommand)
                    {
                        Console.WriteLine(result.ErrorReason);
                        var emb = new InfoEmbed("ERROR:", result.ErrorReason).Build();
                        await message.Channel.SendMessageAsync("", embed: emb);
                    }
                    else
                    {
                        if (user.GetItemList().Contains(message.Content.Replace(";", "")))
                        {
                            await message.Channel.SendMessageAsync("Nothing happens... *Use `;suggest [suggestion]` if you have an idea for this item!*");
                        }
                    }
                }
                else
                {
                    //give user a chance at a lootbox
                    bool inLM = false;
                    //go through users last command time
                    foreach (var u in Var.lastMessage)
                    {
                        //ensure user is in dictionary
                        if (u.Key == context.User.Id) { inLM = true; break; }
                    }
                    if (inLM == false) Var.lastMessage.Add(context.User.Id, Var.CurrentDate() - new TimeSpan(1, 0, 1));
                    //if chance of lootbox
                    if (Var.lastMessage[context.User.Id] <= Var.CurrentDate() - new TimeSpan(1, 0, 0))
                    {
                        //10% chance at lootbox
                        if (rdm.Next(100) + 1 < 10)
                        {
                            await context.Channel.SendMessageAsync(":package: `A lootbox appears in your inventory! (package)`");
                            Functions.GetUser(context.User).GiveItem("package");
                        }
                    }
                    //set last message time to now
                    Var.lastMessage[context.User.Id] = Var.CurrentDate();
                }
            }
            else if (message.MentionedUsers.First().Id == client.CurrentUser.Id && message.Author.Id != client.CurrentUser.Id && Var.responding && ((message.Channel as IGuildChannel).Guild.Id != Constants.Guilds.YORK_UNIVERSITY || message.Channel.Id == Constants.Channels.COMMANDS))
                Functions.Respond(message);
            else if ((message.Channel as IGuildChannel).Guild.Id != Constants.Guilds.YORK_UNIVERSITY && !Var.responding)
                Functions.Respond(message);
            else return;

        }
        public async Task HandleJoin(SocketGuildUser user)
        {
            if (Var.LockDown && user.Guild.Id == Constants.Guilds.YORK_UNIVERSITY) await user.KickAsync();
            else await (user.Guild.GetChannel(Constants.Channels.GENERAL_SLOW) as IMessageChannel).SendMessageAsync($"{user.Mention}! Welcome to {user.Guild.Name}! Go to <#271843457121779712> to get a role using the `?ranks` command.");
        }
        public async Task HandleLeave(SocketGuildUser user)
        {
            if (!Var.LockDown) await (user.Guild.GetChannel(Constants.Channels.GENERAL_SLOW) as IMessageChannel).SendMessageAsync($"{user.Username} has left the server.");
        }
        public async Task HandleDelete(Cacheable<IMessage, ulong> cache, ISocketMessageChannel channel)
        {
            var msg = cache.Value;
            var id = (msg.Author as IGuildUser).Guild.Id;
            if ((id == Constants.Guilds.BASSIC || id == Constants.Guilds.YORK_UNIVERSITY) & msg.Author.Id != client.CurrentUser.Id && !Var.purging && msg.Content != ";bomb")
            {
                JEmbed emb = new JEmbed();
                emb.Title = msg.Author.Username + "#" + msg.Author.Discriminator;
                emb.Author.Name = "MESSAGE DELETED";
                emb.ThumbnailUrl = msg.Author.GetAvatarUrl();
                emb.Description = msg.Content;

                string attachURL = null;
                if (msg.Attachments.Count > 0) attachURL = msg.Attachments.FirstOrDefault().ProxyUrl;
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

                ulong msgChan = 0;

                if (id == Constants.Guilds.YORK_UNIVERSITY) msgChan = Constants.Channels.DELETED_MESSAGES;
                else msgChan = Constants.Channels.ASS_DELETED_MESSAGES;

                var chan = client.GetChannel(msgChan) as IMessageChannel;
                await chan.SendMessageAsync("", embed: emb.Build());
            }
        }
        public async Task HandleEdit(Cacheable<IMessage, ulong> cache, SocketMessage msg, ISocketMessageChannel channel)
        {
            if (msg.Content == cache.Value.Content) return;
            if ((msg.Channel as IGuildChannel).Guild.Id == Constants.Guilds.YORK_UNIVERSITY && Functions.Filter(msg.Content))
            {
                await msg.DeleteAsync();
                return;
            }

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
                Discord.Rest.RestUserMessage message = null;
                foreach (IMessage msg in Var.awaitingHelp)
                {
                    if (msg.Id == cache.Value.Id)
                    {
                        if (react.Emote.Name == Constants.Emotes.HAMMER.Name) tag = "[MOD]";
                        else if (react.Emote.Name == Constants.Emotes.DIE.Name) tag = "[FUN]";
                        else if (react.Emote.Name == Constants.Emotes.QUESTION.Name) tag = "[OTHER]";
                        else if (react.Emote.Name == Constants.Emotes.BRADY.Name) tag = "[BRADY]";
                        message = msg as Discord.Rest.RestUserMessage;
                        Var.awaitingHelp.Remove(msg);
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
                                x.Text = c.Summary.Replace(tag + " ", "");
                            }));
                        }
                    }
                    await message.ModifyAsync(x => x.Embed = emb.Build());
                    await message.RemoveAllReactionsAsync();
                }

            }
        }
        public async Task HandleVoiceUpdate(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var gUser = user as IGuildUser;
            if (gUser.GuildId == Constants.Guilds.YORK_UNIVERSITY)
            {
                if (newState.VoiceChannel != null) await gUser.AddRoleAsync(gUser.Guild.GetRole(Constants.Roles.TTS));
                else await gUser.RemoveRoleAsync(gUser.Guild.GetRole(Constants.Roles.TTS));
            }
        }
    }
}

