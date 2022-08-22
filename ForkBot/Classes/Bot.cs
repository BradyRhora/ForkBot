﻿using System;
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
using HtmlAgilityPack;
using System.Data.SQLite;

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
                Var.IDEnd = rdm.Next(10);
                Console.WriteLine($"ForkBot successfully intialized with debug code [{Var.DebugCode}]");
                Var.startTime = Var.CurrentDate();

                int strikeCount = (Var.CurrentDate() - Constants.Dates.REBIRTH).Days;
                await client.SetGameAsync(strikeCount + " days since REBIRTH", type: ActivityType.Watching);
                Timers.RemindTimer = new Timer(Timers.Remind, null, 1000 * 30, 1000 * 60);
                Timers.BidTimer = new Timer(Timers.BidTimerCallBack, null, 1000 * 30, 1000 * 60);
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
                Console.Read();
            }
        }
        public async Task InstallCommands()
        {
            client.MessageReceived += HandleMessage;
            //client.UserJoined += HandleJoin;
            //client.UserLeft += HandleLeave;
            //client.MessageDeleted += HandleDelete;
            client.ReactionAdded += HandleReact;
            //client.MessageUpdated += HandleEdit;
            //client.UserVoiceStateUpdated += HandleVoiceUpdate;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services: null);
        }

        DateTime lastDay = Var.CurrentDate();
        List<ulong> newUsers = new List<ulong>();


        public async Task HandleMessage(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;
            bool isDM = await Functions.isDM(message as IMessage);
            if (isDM && !message.Content.StartsWith(";"))
            {
                Console.WriteLine(message.Author.Username + " says:\n" + message.Content);
            }
            if (isDM && Var.LockDM) { Console.WriteLine(message.Author.Username + " [" + message.Author.Id + "] attempted to use a command in DM's:\n'"+message.Content+"'"); return; }
            if (message == null) return;
            if (message.Author.Id == client.CurrentUser.Id) return; //doesn't allow the bot to respond to itself
            if (Var.DebugMode && message.Author.Id != Constants.Users.BRADY && Var.DebugUsers.Where(x => x.Id == message.Author.Id).Count() <= 0) return;
            
            if (!Var.DebugMode && message.Channel.Id == Constants.Channels.DEBUG) return;
            var user = User.Get(message.Author);

            #region Pre-Command Functions

            //Daily Updates (strike, game notify)
            if (lastDay.DayOfYear < Var.CurrentDate().DayOfYear)
            {
                //status update
                int strikeCount = (Var.CurrentDate() - Constants.Dates.REBIRTH).Days;
                await client.SetGameAsync(strikeCount + " days since REBIRTH", type: ActivityType.Watching);
            }

            //checks if message contains any blocked words ## DISABLED ##
            /*if (!isDM && (message.Channel as IGuildChannel).Guild.Id == Constants.Guilds.YORK_UNIVERSITY && Functions.Filter(message.Content))
            {
                await message.DeleteAsync();
                return;
            }*/

            if (Var.blockedUsers.Where(x=>x.Id == message.Author.Id).Count() > 0) return; //prevents "blocked" users from using the bot

            //present stuff
            if (Var.presentWaiting && message.Content == Convert.ToString(Var.presentNum))
            {
                Var.presentWaiting = false;
                var presents = DBFunctions.GetItemIDList();
                int presID;
                do
                {
                    var presIndex = rdm.Next(presents.Count());
                    presID = presents[presIndex];
                } while (!DBFunctions.ItemIsPresentable(presID));
                Var.present = DBFunctions.GetItemName(presID);
                Var.rPresent = Var.present;
                var presentName = Var.present;
                var pMessage = DBFunctions.GetItemDescription(presID);
                var msg = $"{message.Author.Username}! You got...\nA {Func.ToTitleCase(presentName.Replace('_', ' '))}! {DBFunctions.GetItemEmote(presID)} {pMessage}";
                user.GiveItem(Var.present);

                if (Var.replaceable)
                {
                    msg += $"\nDon't like this gift? Press {Var.presentNum} again to replace it once!";
                    Var.replacing = true;
                    Var.presentReplacer = message.Author;
                }
                await message.Channel.SendMessageAsync(msg);
            }
            else if (Var.replaceable && Var.replacing && message.Content == Convert.ToString(Var.presentNum) && message.Author == Var.presentReplacer)
            {
                if (user.HasItem(Var.present))
                {
                    user.RemoveItem(Var.present);
                    await message.Channel.SendMessageAsync($":convenience_store: {DBFunctions.GetItemEmote(Var.present)} :runner: \nA **new** present appears! :gift: Press {Var.presentNum} to open it!");
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

            //April fools covid edition
            /*if (DateTime.Now.Month == 4 && (message.Channel as IGuildChannel).Guild.Id == Constants.Guilds.YORK_UNIVERSITY)
            {
                var guildUser = message.Author as IGuildUser;
                var infected = guildUser.RoleIds.Contains(Constants.Roles.INFECTED);

                if (infected)
                {

                    var msgs = message.Channel.GetCachedMessages(20).ToArray();
                    bool next = false;
                    IGuildUser lastUser = null;
                    for (int i = 0; i < msgs.Count(); i++)
                    {
                        if (next)
                        {
                            lastUser = msgs[i].Author as IGuildUser;
                            break;
                        }
                        else if (msgs[i].Id == message.Id)
                            next = true;
                    }

                    if (guildUser.Id != lastUser.Id)
                    {
                        var luInfected = lastUser.RoleIds.Contains(Constants.Roles.INFECTED);
                        if (infected && !luInfected)
                        {
                            await lastUser.AddRoleAsync(guildUser.Guild.GetRole(Constants.Roles.INFECTED));
                            Console.WriteLine("Infected " + lastUser.Username);
                        }
                        //else if (!infected && luInfected)
                        //{
                        //    await guildUser.AddRoleAsync(guildUser.Guild.GetRole(Constants.Roles.INFECTED));
                        //    Console.WriteLine("Infected " + guildUser.Username);
                        //}
                    }
                }
            }*/

            //Doesnt allow bot usage in "blocked" channels
            //ulong[] blockedChannels = { Constants.Channels.GENERAL_SLOW, Constants.Channels.GENERAL_TRUSTED, Constants.Channels.NEWS_DEBATE, Constants.Channels.LIFESTYLE };
            //if (!isDM && (message.Channel as IGuildChannel).Guild.Id == Constants.Guilds.YORK_UNIVERSITY && (blockedChannels.Contains(message.Channel.Id)) && !(message.Author as IGuildUser).RoleIds.Contains(Constants.Roles.MOD) && !(message.Author as IGuildUser).RoleIds.Contains(Constants.Roles.BOOSTER)) return;
            #endregion

            int argPos = 0;
            //detect and execute commands
            if (message.HasCharPrefix(';', ref argPos))
            {
                // new user prevention
                var userCreationDate = message.Author.CreatedAt;
                var existenceTime = DateTime.UtcNow.Subtract(userCreationDate.DateTime);
                var week = new TimeSpan(7, 0, 0, 0);
                if (existenceTime < week && !message.Content.Contains("verify"))
                {
                    if (!newUsers.Contains(message.Author.Id))
                    {
                        newUsers.Add(message.Author.Id);
                        await message.Author.SendMessageAsync("Hi there! Welcome to Discord. In order to avoid bot abuse, your account must be older than a few days to use the bot.\n" +
                            "If you don't understand, just message <@108312797162541056> about it.\nThanks!");
                    }
                    return;
                }

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
                        if (user.HasItem(message.Content.Replace(";", "")))
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
                            User.Get(context.User).GiveItem("package");
                        }
                    }
                    //set last message time to now
                    Var.lastMessage[context.User.Id] = Var.CurrentDate();
                }
            }
            else return;

        }
        public async Task HandleJoin(SocketGuildUser user)
        {
            if (Var.DebugMode) return;
            if (Var.LockDown && user.Guild.Id == Constants.Guilds.YORK_UNIVERSITY)
            {
                await user.KickAsync();
                await (user.Guild.GetChannel(Constants.Channels.REPORTED) as ITextChannel).SendMessageAsync($"LOCKDOWN:\n```Auto kicked: {user.Username}#{user.DiscriminatorValue}\n```");
            }
            else
            {
                await (user.Guild.GetChannel(Constants.Channels.LANDING) as IMessageChannel).SendMessageAsync($"{user.Mention}! Welcome to {user.Guild.Name}! To gain access to all channels, check #landing-rules for more information. Enjoy!");
                await (user.Guild.GetChannel(Constants.Channels.GENERAL_SLOW) as IMessageChannel).SendMessageAsync($"{user.Mention}! Welcome to {user.Guild.Name}!");
            }
        }
        public async Task HandleLeave(SocketGuild guild, SocketUser user)
        {
            if (Var.DebugMode) return;
            if (!Var.LockDown && guild.Id == Constants.Guilds.YORK_UNIVERSITY) await (guild.GetChannel(Constants.Channels.GENERAL_SLOW) as IMessageChannel).SendMessageAsync($"{user.Username} has left the server.");
        }
        public async Task HandleDelete(Cacheable<IMessage, ulong> cache, Cacheable<IMessageChannel,ulong> channel)
        {
            var msg = cache.Value;
            if (msg == null) return;
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
            if (msg == null || cache.Value == null) return;
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
        public async Task HandleReact(Cacheable<IUserMessage, ulong> cache, Cacheable<IMessageChannel, ulong> channel, SocketReaction react)
        {
            if (cache.Value == null) return;
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


                var awaitingUser = Var.awaitingVerifications.Where(x => x.Message.Id == react.MessageId).FirstOrDefault();
                if (awaitingUser != null)
                {
                    IGuildUser user = awaitingUser.User as IGuildUser;
                    await user.AddRolesAsync(awaitingUser.Roles);
                    await user.AddRoleAsync(user.Guild.GetRole(Constants.Roles.VERIFIED));

                    await (client.GetChannel(Constants.Channels.REPORTED) as IMessageChannel).SendMessageAsync("Successfully verified.");
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

