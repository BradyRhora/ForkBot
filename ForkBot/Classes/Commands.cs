using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DuckDuckGo.Net;
using OxfordDictionariesAPI;
using HtmlAgilityPack;
using System.Drawing;
using System.Net;
using ImageProcessor;
using ImageProcessor.Imaging;
using System.Text.RegularExpressions;
using System.Xml;
namespace ForkBot
{
    public class Commands : ModuleBase
    {
        Random rdm = new Random();
        readonly Exception NotBradyException = new Exception("This command can only be used by Brady.");

        #region Useful

        [Command("help"), Summary("Displays commands and descriptions.")]
        public async Task Help()
        {
            JEmbed emb = new JEmbed();
            emb.Author.Name = "ForkBot Commands";
            emb.ThumbnailUrl = Context.User.AvatarId;
            if (Context.Guild != null) emb.ColorStripe = Functions.GetColor(Context.User);
            else emb.ColorStripe = Constants.Colours.DEFAULT_COLOUR;

            emb.Description = "Select the emote that corresponds to the commands you want to see.";

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Text = ":hammer:";
                x.Header = "MOD COMMANDS";
                x.Inline = true;
            }));

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Text = ":game_die:";
                x.Header = "FUN COMMANDS";
                x.Inline = true;
            }));

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Text = ":question:";
                x.Header = "OTHER COMMANDS";
                x.Inline = true;
            }));
            
            if (Context.User.Id == Constants.Users.BRADY)
            {
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Text = "<:brady:465359176575614980>";
                    x.Header = "BRADY COMMANDS";
                    x.Inline = true;
                }));
            }


            var msg = await Context.Channel.SendMessageAsync("", embed: emb.Build());

            await msg.AddReactionAsync(Constants.Emotes.HAMMER);
            await msg.AddReactionAsync(Constants.Emotes.DIE);
            await msg.AddReactionAsync(Constants.Emotes.QUESTION);
            if (Context.User.Id == Constants.Users.BRADY) await msg.AddReactionAsync(Constants.Emotes.BRADY);
            Var.awaitingHelp.Add(msg);
        }

        [Command("whatis"), Alias(new string[] { "wi" }), Summary("Don't know what something is? Find out!")]
        public async Task WhatIs([Remainder]string thing)
        {
            var results = new Search().Query(thing, "ForkBot");
            QueryResult result = null;
            if (results.Abstract == "" && results.RelatedTopics.Count > 0) result = results.RelatedTopics[0];

            if (result != null)
            {
                JEmbed emb = new JEmbed();
                emb.Title = thing;
                emb.Description = result.Text;
                emb.ImageUrl = result.Icon.Url;
                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            else await Context.Channel.SendMessageAsync("No results found!");

        }

        [Command("define"), Alias(new string[] { "def" }), Summary("Returns the definiton for the inputted word.")]
        public async Task Define([Remainder]string word)
        {
            OxfordDictionaryClient client = new OxfordDictionaryClient("45278ea9", "c4dcdf7c03df65ac5791b67874d956ce");
            var result = await client.SearchEntries(word, CancellationToken.None);
            if (result != null)
            {
                var senses = result.Results[0].LexicalEntries[0].Entries[0].Senses[0];

                JEmbed emb = new JEmbed();
                emb.Title = Func.ToTitleCase(word);
                emb.Description = Char.ToUpper(senses.Definitions[0][0]) + senses.Definitions[0].Substring(1) + ".";
                emb.ColorStripe = Constants.Colours.YORK_RED;
                if (senses.Examples != null)
                {
                    emb.Fields.Add(new JEmbedField(x =>
                    {
                        x.Header = "Examples:";
                        string text = "";
                        foreach (OxfordDictionariesAPI.Models.Example eg in senses.Examples)
                        {
                            text += $"\"{Char.ToUpper(eg.Text[0]) + eg.Text.Substring(1)}.\"\n";
                        }
                        x.Text = text;
                    }));
                }
                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            else await Context.Channel.SendMessageAsync($"Could not find definition for: {word}.");
        }

        [Command("professor"), Alias(new string[] { "prof", "rmp" }), Summary("Check out a professors rating from RateMyProfessors.com!")]
        public async Task Professor([Remainder]string name)
        {
            HtmlWeb web = new HtmlWeb();

            string link = "http://www.ratemyprofessors.com/search.jsp?query=" + name.Replace(" ", "%20");
            var page = web.Load(link);
            //var node = page.DocumentNode.SelectSingleNode("//*[@id=\"searchResultsBox\"]/div[2]/ul/li[1]");
            var node = page.DocumentNode.SelectSingleNode("//*[@id=\"searchResultsBox\"]/div[2]/ul/li/a");
            if (node != null)
            {
                var newLink = "http://www.ratemyprofessors.com" + node.Attributes[0].Value;
                page = web.Load(newLink);

                var rating = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[1]/div/div/div").InnerText;
                var takeAgain = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[2]/div[1]/div").InnerText;
                var difficulty = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[2]/div[2]/div").InnerText;
                var titleText = page.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
                string profName = titleText.Split(' ')[0] + " " + titleText.Split(' ')[1];
                string university = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[1]/div[1]/div[1]/div[3]/h2/a").InnerText;
                university = university.Replace(" (all campuses)", "");
                var tagBox = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[2]/div[2]");
                List<string> tags = new List<string>();
                for (int i = 0; i < tagBox.ChildNodes.Count(); i++)
                {
                    if (tagBox.ChildNodes[i].Name == "span") tags.Add(tagBox.ChildNodes[i].InnerText);
                }
                
                JEmbed emb = new JEmbed();

                emb.Title = profName + " - " + university;
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Rating:";
                    x.Text = rating;
                    x.Inline = true;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Difficulty:";
                    x.Text = difficulty;
                    x.Inline = true;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Would take again?:";
                    x.Text = takeAgain;
                    x.Inline = true;
                }));

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Top Tags:";
                    string text = "";
                    foreach (string s in tags)
                    {
                        text += s;
                    }
                    x.Text = text;
                    x.Inline = false;
                }));

                emb.ColorStripe = Constants.Colours.YORK_RED;
                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            else
            {
                await Context.Channel.SendMessageAsync("Professor not found!");
            }
        }

        [Command("course"), Summary("Shows details for course from inputted course code.")]
        public async Task Course([Remainder] string code)
        {
            try
            {
                //formats course code correctly
                if (Regex.IsMatch(code, "([A-z]{2,4} *[0-9]{4})"))
                {
                    var splits = Regex.Split(code, "(\\d+|\\D+)").Where(x=>x!="").ToArray();
                    code = splits[0].Trim() + " " + splits[1].Trim();
                }

                Course course = new Course(code);
                               

                JEmbed emb = new JEmbed();
                emb.Title = course.GetTitle();
                emb.Description = course.GetDescription() + "\n\n";
                emb.ColorStripe = Constants.Colours.YORK_RED;

                foreach(CourseDay day in course.GetSchedule().Days)
                {
                    emb.Description += $"\n{day.Term} {day.Section} - {day.Professor}\n";
                    foreach(var dayTime in day.DayTimes) emb.Description += $"\n{dayTime.Key} - {dayTime.Value}";
                    emb.Description += "\n";
                }

                

                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            catch (Exception) { await Context.Channel.SendMessageAsync("There was an error loading the course page. (Possibly not available this term)"); }

        }

        [Command("courselist"), Summary("Displays all courses for the inputted subject.")]
        public async Task CourseList(string subject, int page = 1)
        {
            if (page < 1)
            {
                await ReplyAsync("Please input a valid page number.");
                return;
            }

            string[] courses = File.ReadAllLines("Files/courselist.txt");
            string list = "";
            foreach(string course in courses)
            {
                var data = course.Split('/');
                if (data.Count() > 1)
                    if (data[1].StartsWith(subject.ToUpper())) list += course + "\n";
            }

            if (list == "")
            {
                await ReplyAsync($"No courses found with subject: {subject}");
                return;
            }

            string[] msgs = Functions.SplitMessage(list);


            if (page > msgs.Count()) page = msgs.Count()-1;

            JEmbed courseEmb = new JEmbed();
            courseEmb.Author.Name = $"{subject.ToUpper()} Course List";
            courseEmb.Author.IconUrl = Constants.Images.ForkBot;
            courseEmb.ColorStripe = Constants.Colours.YORK_RED;

            courseEmb.Description = msgs[page-1];

            courseEmb.Footer.Text = $"Page {page}/{msgs.Count()} (Use ';courselist {subject.ToUpper()} #' and replace the number with a page number!)";

            await ReplyAsync("", embed: courseEmb.Build());
        }

        [Command("suggest"), Summary("Suggest something for ForkBot, whether it's an item, an item's function, a new command, or anything else! People who abuse this will be blocked from using it.")]
        public async Task Suggest([Remainder] string suggestion)
        {
            var brady = Bot.client.GetUser(Constants.Users.BRADY);
            if (Properties.Settings.Default.sBlocked == null) { Properties.Settings.Default.sBlocked = new System.Collections.Specialized.StringCollection(); Properties.Settings.Default.Save(); }
            if (Properties.Settings.Default.sBlocked.Contains(Convert.ToString(Context.User.Id))) return;
            await brady.SendMessageAsync("", embed: new InfoEmbed("SUGGESTION FROM: " + Context.User.Username, suggestion).Build());
            await Context.Channel.SendMessageAsync("Suggestion submitted.");
        }

        [Command("updates"), Summary("See the most recent update log.")]
        public async Task Updates()
        {
            await Context.Channel.SendMessageAsync("```\nFORKBOT BETA CHANGELOG 2.41\n-Some bug fixes\n-added shop help text\n-buffed moneybag\n-fixed iteminfo sell price\n-fixed custom emotes in trades"+
                "\n-buffed lootboxes\n-fixed bug with ;course that wouldnt load courses with cancelled classes\n-added ;remind command for users\n-started forkparty\n-removed present replacement animation"+
                "\n-fixed forkbot DMs(nvm)\n-present shows record claims\n-parameter for ;top can now be an item\n-trusted system\n-added ;freemarket and fixed bugs\n" +
                "-lots of bug fixes regarding ;fm, ;tips, ;weed, and ;stopwatch.```");
        }

        [Command("stats"), Summary("See stats regarding Forkbot.")]
        public async Task Stats()
        {
            var guilds = Bot.client.Guilds;
            int guildCount = guilds.Count();
            int userCount = 0;
            foreach(IGuild g in guilds)
            {
                userCount += (await g.GetUsersAsync()).Count();
            }
            var uptime = Var.CurrentDate() - Var.startTime;

            JEmbed emb = new JEmbed();
            emb.Title = "ForkBot Stats";
            emb.Description = $"ForkBot is developed by Brady#0010 for use in the York University Discord server.\nIt has many uses, such as professor lookup, course lookup, word defining, and many fun commands.";
            emb.ColorStripe = Constants.Colours.YORK_RED;
            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Users";
                x.Text = $"Serving {userCount} users in {guildCount} guilds.";
            }));
            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Uptime";
                x.Text = $"{uptime.Days} days, {uptime.Hours} hours, {uptime.Minutes} minutes, {uptime.Seconds} seconds.";
            }));
            await ReplyAsync("", embed: emb.Build());
        }
        
        [Command("remind"), Summary("Sets a message to remind you of in the specified amount of time."), Alias(new string[] { "reminder", "rem" })]
        public async Task Remind([Remainder] string parameters = "")
        {
            if (parameters == "")
            {
                await ReplyAsync("This command reminds you of the message you choose, in the amount of time that you specify. You may have max five reminders at a time.\n" +
                                 "Seperate the message you want to be reminded of and the amount of time with the keyword `in`. If you have multiple `in`'s the last one will be used.\n" +
                                 "eg: `;remind math1019 midterm in 6 days and 3 hours`\nYou can use any combination of days, hours, minutes. "+
                                 "Seperate each using either a comma or the word `and`.\n"+
                                 "Use `;reminderlist` to view your reminders and `;deletereminder [#]` with the reminder number to delete it.");
            }
            else if (parameters.Contains(" in "))
            {
                var currentReminders = File.ReadAllLines("Files/userreminders.txt").Where(x => x.StartsWith(Convert.ToString(Context.User.Id)));
                if (currentReminders.Count() >= 5)
                {
                    await ReplyAsync("You already have 5 reminders, which is the maximum.");
                }
                else
                {
                    string[] split = parameters.Split(new string[] { " in " }, StringSplitOptions.None);
                    string reminder = "";
                    if (split.Count() == 2) reminder = split[0];
                    else
                    {
                        for (int i = 0; i < split.Count() - 1; i++)
                        {
                            reminder += split[i] + " in ";
                        }
                        reminder = reminder.Substring(0, reminder.Length - 4);
                    }

                    reminder = reminder.Replace("//#//", "");

                    string time = split[split.Count() - 1];
                    string[] splitTimes = time.Split(new string[] { ", and ", " and ", ", "  }, StringSplitOptions.None);
                    TimeSpan remindTime = new TimeSpan(0, 0, 0);
                    bool stop = false;
                    foreach (string t in splitTimes)
                    {
                        var timeData = t.Split(' ');
                        if (timeData.Count() > 2)
                        {
                            stop = true;
                            break;
                        }
                        var format = timeData[1].ToLower().TrimEnd('s');
                        var amount = timeData[0];

                        switch (format)
                        {
                            case "day":
                                remindTime = remindTime.Add(new TimeSpan(Convert.ToInt32(amount), 0, 0, 0));
                                break;
                            case "hour":
                                remindTime = remindTime.Add(new TimeSpan(Convert.ToInt32(amount), 0, 0));
                                break;
                            case "minute":
                                remindTime = remindTime.Add(new TimeSpan(0, Convert.ToInt32(amount), 0));
                                break;
                            default:
                                stop = true;
                                break;
                        }
                    }

                    if (stop) await ReplyAsync("Invalid time format, make sure time formats are spelt correctly.");
                    else
                    {
                        DateTime remindAt = Var.CurrentDate() + remindTime;
                        string timeString = Functions.DateTimeToString(remindAt);
                        string writeString = Context.User.Id + "//#//" + reminder + "//#//" + timeString + "\n";

                        File.AppendAllText("Files/userreminders.txt", writeString);
                        await ReplyAsync("Reminder added.");
                    }
                }
            }
            else await ReplyAsync("Invalid format, make sure you have the word `in` with spaces on each side.");
        }

        [Command("reminderlist"), Alias(new string[] { "reminders" })]
        public async Task ReminderList()
        {
            var currentReminders = File.ReadAllLines("Files/userreminders.txt").Where(x => x.StartsWith(Convert.ToString(Context.User.Id)));
            if (currentReminders.Count() > 0)
            {
                string msg = "Here are your current reminders:\n```";
                for (int i = 0; i < currentReminders.Count(); i++)
                {
                    msg += $"[{i + 1}]" + currentReminders.ElementAt(i).Replace("//#//", " ").Replace($"{Context.User.Id}", "").Trim() + "\n";
                }
                await ReplyAsync(msg + "\n```\nUse `;deletereminder #` to delete a reminder!");
            }
            else await ReplyAsync("You currently have no reminders.");
        }
        
        [Command("deletereminder"),Alias(new string[] { "delreminder" })]
        public async Task DeleteReminder(int reminderID)
        {
            var reminders = File.ReadAllLines("Files/userreminders.txt");
            int idCount = 0;
            bool deleted = false;
            for(int i = 0; i < reminders.Count(); i++)
            {
                if (reminders[i].StartsWith($"{Context.User.Id}"))
                {
                    idCount++;
                    if (idCount == reminderID)
                    {
                        reminders[i] = "";
                        deleted = true;
                        break;
                    }
                }
            }

            if (deleted)
            {
                File.Delete("Files/userreminders.txt");
                File.WriteAllLines("Files/userreminders.txt", reminders.Where(x => x != ""));
                await ReplyAsync("Reminder deleted.");
            }
            else
            {
                await ReplyAsync("Reminder not found, are you sure you have a reminder with an ID of " + reminderID + "? Use `;reminderlist` to check.");
            }
        }



        #endregion

        #region Item Commands


        [Command("sell"), Summary("[FUN] Sell items from your inventory.")]
        public async Task Sell(params string[] items)
        {
            var u = Functions.GetUser(Context.User);
            string msg = "";
            var itemList = Functions.GetItemList();
            if (items.Count() == 1 && items[0] == "all") await ReplyAsync("Are you sure you want to sell **all** of your items? Use `;sell allforreal` if so.");
            else if (items.Count() == 1 && items[0] == "allforreal")
            {
                int coinGain = 0;
                foreach (string item in u.GetItemList())
                {
                    u.RemoveItem(item);
                    int price = 0;
                    foreach (string line in itemList)
                    {
                        if (line.Split('|')[0] == item)
                        {
                            if (!line.Split('|')[2].Contains("-"))
                                price = (int)(Convert.ToInt32(line.Split('|')[2]) * Constants.Values.SELL_VAL);
                            else price = 10;
                            coinGain += price;
                            break;
                        }
                    }
                }
                u.GiveCoins(coinGain);
                msg = $"You have sold ***ALL*** of your items for {coinGain} coins.";
            }
            else
            {
                foreach (string item in items)
                {
                    if (u.GetItemList().Contains(item))
                    {
                        u.RemoveItem(item);
                        int price = 0;
                        foreach (string line in itemList)
                        {
                            if (line.Split('|')[0] == item)
                            {
                                if (!line.Split('|')[2].Contains("-"))
                                    price = (int)(Convert.ToInt32(line.Split('|')[2]) * Constants.Values.SELL_VAL);
                                else price = 10;
                                break;
                            }
                        }

                        u.GiveCoins(price);
                        msg += $"You successfully sold your {item} for {price} coins!\n";

                    }
                    else msg += $"You do not have an item called {item}!\n";
                }
            }

            var msgs = Functions.SplitMessage(msg);
            foreach (string m in msgs) await ReplyAsync(m);
        }

        [Command("trade"), Summary("[FUN] Initiate a trade with another user!")]
        public async Task Trade(IUser user)
        {
            if (Functions.GetTrade(Context.User) == null && Functions.GetTrade(user) == null)
            {
                if (user.Id == Context.User.Id)
                {
                    await ReplyAsync("You cannot trade yourself.");
                    return;
                }
                Var.trades.Add(new ItemTrade(Context.User, user));
                await Context.Channel.SendMessageAsync("", embed: new InfoEmbed("TRADE INVITE",
                    user.Mention + "! " + Context.User.Username + " has invited you to trade."
                    + " Type ';trade accept' to accept or ';trade deny' to deny!").Build());
            }
            else await Context.Channel.SendMessageAsync("Either you or the person you are attempting to trade with is already in a trade!"
                                                    + " If you accidentally left a trade going, use `;trade cancel` to cancel the trade.");
        }

        [Command("trade")]
        public async Task Trade(string command, string param = "")
        {
            bool showMenu = false;
            var trade = Functions.GetTrade(Context.User);
            if (trade != null)
            {
                switch (command)
                {
                    case "accept":
                        if (!trade.Accepted && Context.User.Id != trade.Starter().Id) trade.Accept();
                        showMenu = true;
                        break;
                    case "deny":
                        if (!trade.Accepted)
                        {
                            await Context.Channel.SendMessageAsync("", embed: new InfoEmbed("TRADE DENIED", $"{trade.Starter().Mention}, {Context.User.Username} has denied the trade request.").Build());
                            Var.trades.Remove(trade);
                        }
                        break;
                    case "add":
                        if (param != "")
                        {
                            var success = trade.AddItem(Context.User, param);
                            if (success == false)
                            {
                                if (trade.Accepted)
                                    await Context.Channel.SendMessageAsync("Unable to add item. Are you sure you have it?");
                                else
                                    await ReplyAsync("The other user has not accepted the trade yet.");
                            }
                            else showMenu = true;
                        }
                        else await Context.Channel.SendMessageAsync("Please specify the item to add!");
                        break;
                    case "finish":
                        trade.Confirm(Context.User);
                        if (trade.IsCompleted()) Var.trades.Remove(trade);
                        else await Context.Channel.SendMessageAsync("Awaiting confirmation from other user.");
                        break;
                    case "cancel":
                        trade.Cancel();
                        await Context.Channel.SendMessageAsync("", embed: new InfoEmbed("TRADE CANCELLED", $"{Context.User.Username} has cancelled the trade. All items have been returned.").Build());
                        break;
                }
            }
            else await Context.Channel.SendMessageAsync("You are not currently part of a trade.");

            if (showMenu) await Context.Channel.SendMessageAsync("", embed: trade.CreateMenu());

            if (trade.IsCompleted())
            {
                await Context.Channel.SendMessageAsync("", embed: new InfoEmbed("TRADE SUCCESSFUL", "The trade has been completed successfully.").Build());
                Var.trades.Remove(trade);
            }
        }

        [Command("donate"), Summary("[FUN] Give the specified user some of your coins or items!")]
        public async Task Donate(IUser user, int donation)
        {
            int coins = donation;
            User u1 = Functions.GetUser(Context.User);
            if (donation <= 0) await ReplyAsync("Donation must be greater than 0 coins.");
            else
            {
                if (u1.GetCoins() >= coins)
                {
                    u1.GiveCoins(-coins);
                    Functions.GetUser(user).GiveCoins(coins);
                    await ReplyAsync($":moneybag: {user.Mention} has been given {coins} of your coins!");
                }
                else await ReplyAsync("You don't have enough coins.");
            }
        }
        
        [Command("give"), Summary("[FUN] Give the specified user some of your items!")]
        public async Task give(IUser user, params string[] donation)
        {
            User u1 = Functions.GetUser(Context.User);
            User u2 = Functions.GetUser(user);

            string msg = $"{user.Mention}, {Context.User.Mention} has given you:\n";
            string donations = "";
            string fDonations = "";
            foreach(string item in donation)
            {
                if (u1.GetItemList().Contains(item))
                {
                    u1.RemoveItem(item);
                    u2.GiveItem(item);
                    donations += $"A(n) {item}!\n";
                }
                else fDonations += $"~~A(n) {item}~~ {Context.User.Mention}, you do not have a(n) {item}.\n";
            }

            if (donations == "") msg = $"{Context.User.Mention}, you do not have any of the inputted item(s).";
            else msg += donations += fDonations;

            await ReplyAsync(msg);
        }

        [Command("shop"), Summary("[FUN] Open the shop and buy stuff! New items each day."), Alias(new string[] { "buy" })]
        public async Task Shop([Remainder] string command = null)
        {
            var u = Functions.GetUser(Context.User);
            DateTime day = new DateTime();
            DateTime currentDay = new DateTime();
            if (Var.currentShop != null)
            {
                day = Var.currentShop.Date();
                currentDay = Var.CurrentDate();
            }
            if (Var.currentShop == null || Math.Abs(day.Hour-currentDay.Hour) >= 4)
            {
                Var.currentShop = new Shop();
            }

            List<string> itemNames = new List<string>();
            foreach (string item in Var.currentShop.items) itemNames.Add(item.Split('|')[0]);



            if (command == null)
            {
                var emb = Var.currentShop.Build();
                emb.Footer.Text = $"You have: {u.GetCoins()} coins.\nTo buy an item, use `;shop [item]`.";
                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            else if (itemNames.Contains(command.ToLower().Replace(" ", "_")))
            {
                foreach (string item in Var.currentShop.items)
                {
                    if (item.Split('|')[0] == command.ToLower().Replace(" ", "_"))
                    {
                        var data = item.Split('|');
                        string name = data[0];
                        string desc = data[1];
                        int price = Convert.ToInt32(data[2]);
                        if (price < 0) price *= -1;
                        int stock = Var.currentShop.stock[Var.currentShop.items.IndexOf(item)];
                        if (Convert.ToInt32(u.GetCoins()) >= price && stock > 0)
                        {
                            stock--;
                            Var.currentShop.stock[Var.currentShop.items.IndexOf(item)] = stock;
                            u.GiveCoins(-price);
                            u.GiveItem(name);
                            await Context.Channel.SendMessageAsync($":shopping_cart: You have successfully purchased a(n) {name} {Functions.GetItemEmote(Functions.GetItemData(name))} for {price} coins!");
                        }
                        else await Context.Channel.SendMessageAsync("Either you cannot afford this item or it is not in stock.");
                    }
                }
            }
            else await Context.Channel.SendMessageAsync("Either something went wrong, or this item isn't in stock!");
        }

        [Command("freemarket"), Alias("fm", "market"), Summary("[FUN] Sell items to other users! Choose your own price!")]
        public async Task FreeMarket(params string[] command)
        {
            var user = Functions.GetUser(Context.User);
            bool sort = false, lowest = false, itemParam = false;
            if (command.Count() == 0 || command[0] == "view")
            {
                int page = 1;
                if (command.Count() > 0 && command[0] == "view")
                {
                    if (!int.TryParse(command[1], out page))
                    {
                        if (command[1].ToLower() == "lowest")
                        {
                            sort = true;
                            lowest = true;
                        }
                        else if (command[1].ToLower() == "highest")
                        {
                            sort = true;
                            lowest = false;
                        }
                        else itemParam = true;
                    }

                    if (command.Count() >= 3) int.TryParse(command[command.Count()-1], out page);
                }



                if (page < 1) page = 1;
                
                if (!File.Exists("Files/FreeMarket.txt"))
                {
                    File.AppendAllText("Files/FreeMarket.txt","");
                }

                var itemList = File.ReadAllLines("Files/FreeMarket.txt").ToList();
                IOrderedEnumerable<string> sortedList;
                string[] items = itemList.ToArray();

                if (sort)
                {
                    if (lowest) sortedList = itemList.OrderBy(x => Convert.ToInt32(x.Split('|')[4]));
                    else sortedList = itemList.OrderByDescending(x => Convert.ToInt32(x.Split('|')[4]));
                    items = sortedList.ToArray();
                }

                if (itemParam)
                {
                    items = items.Where(x => x.Split('|')[2] == command[1]).ToArray();
                }
                

                const int ITEMS_PER_PAGE = 10;

                JEmbed emb = new JEmbed();
                emb.Title = "Free Market";
                emb.Description = "To buy an item, use ;fm buy [ID]! For more help and examples, use ;fm help.";
                double pageCount = Math.Ceiling((double)items.Count() / ITEMS_PER_PAGE);
                emb.Footer.Text = $"Page {page}/{pageCount}";
                emb.ColorStripe = Constants.Colours.YORK_RED;

                page -= 1;

                int itemStart = ITEMS_PER_PAGE * page;
                int itemEnd = itemStart + ITEMS_PER_PAGE;

                if (itemStart > items.Count())
                {
                    await ReplyAsync("Invalid page number.");
                    return;
                }

                if (itemEnd > items.Count()) itemEnd = items.Count();
                
                if (items.Count() == 0)
                {
                    emb.Fields.Add(new JEmbedField(x =>
                    {
                        x.Header = "Either the Free Market is empty, or no items match your parameters!";
                        x.Text = "Sorry!";
                    }));
                    emb.Footer.Text = "Page 0/0";
                }

                for (int i = itemStart; i < itemEnd; i++)
                {
                    string[] sData = items[i].Split('|');
                    string id = sData[0];
                    string sellerID = sData[1];
                    string itemName = sData[2];
                    int amount = Convert.ToInt32(sData[3]);
                    int price = Convert.ToInt32(sData[4]);

                    string plural = "";
                    if (amount > 1) plural = "s";


                    emb.Fields.Add(new JEmbedField(x => {
                        x.Header = $"{Functions.GetItemEmote(itemName)} ({amount}) {itemName}{plural} - id: {id}";
                        x.Text = $"<:blank:528431788616318977>:moneybag: {price} Coins\n<:blank:528431788616318977>";
                        x.Inline = false;
                    }));
                }

                await ReplyAsync("", embed: emb.Build());
            }
            else if (command[0] == "help")
            {
                await ReplyAsync("Free Market Help!\n\n" +
                                 "To view the Free Market, use either `;fm` or `;fm view`. You can do `;fm view [page #]` to view other pages.\n" +
                                 "You can also use certain parameters, `lowest`, `highest`, and `[itemname]` to narrow down or sort the Free Market.\n"+
                                 "To buy an item in the free market, use `;fm buy [ID]`. The ID is the characters that appear in the title of the sale in `;fm`\n" +
                                 "To post an item for sale, do ;fm post [item] [price]. You can also include the amount of items you want to sell in the format `[item]*[amount]`\n" +
                                 "To cancel a posting, use `;fm cancel [ID]`\nThere is a 200 coin fee for cancelling posts in order to avoid abuse. This will be automatically charged upon cancellation, if you cannot afford the fee, you cannot cancel.\n\n" +
                                 "Examples:\n\n" +
                                 "`;fm view 3` Views the third Free Market page.\n"+
                                 "`;fm view lowest` Views all items sorted by the lowest price.\n"+
                                 "`;fm view key 5` Views the fifth page of just keys.\n"+
                                 "`;fm post apple 100` Posts 1 apple for sale for 100 coins.\n"+
                                 "`;fm post gun*10 7500` Posts 10 guns for sale for 7500 coins.\n"+
                                 "`;fm buy A1B2C3` buys an item with the ID `A1B2C3`.\n\n"+
                                 "If something still doesn't make sense, just ask Brady.");
            }
            else if (command[0] == "post")
            {
                string[] itemData = command[1].Split('*');

                int amount;
                if (itemData.Count() == 1) amount = 1;
                else int.TryParse(itemData[1], out amount);

                if (amount < 1)
                {
                    await ReplyAsync("You must be posting at least one item.");
                    return;
                }

                string item = itemData[0];
                int price = int.Parse(command[2]);
                if (price < 1)
                {
                    await ReplyAsync("You must be charging at least 1 coin.");
                    return;
                }

                string id = "";

                if (user.GetItemList().Where(x=>x == item).Count() < amount)
                {
                    await ReplyAsync(":x: You either do not have the item, or enough of the item in your inventory. :x:");
                    return;
                }
                for (int i = 0; i < amount; i++) user.RemoveItem(item);


                string key = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                for (int i = 0; i < 5; i++)
                {
                    id += key[rdm.Next(key.Count())];
                }

                string plural = "";
                if (price > 1) plural = "s";
                File.AppendAllText("Files/FreeMarket.txt", $"{id}|{Context.User.Id}|{item}|{amount}|{price}\n");
                await ReplyAsync($"You have successfully posted {amount} {item}(s) for {price} coin{plural}. The sale ID is {id}.");
            }
            else if (command[0] == "buy")
            {
                string id = command[1].ToUpper();
                var items = File.ReadAllLines("Files/FreeMarket.txt");
                for (int i = 0; i < items.Count(); i++)
                {
                    if (items[i].Split('|')[0] == id)
                    {
                        string[] sData = items[i].Split('|');
                        ulong sellerID = Convert.ToUInt64(sData[1]);
                        if (sellerID == Context.User.Id)
                        {
                            await ReplyAsync(":x: You cannot purchase your own posting. :x:");
                            break;
                        }
                        string itemName = sData[2];
                        int amount = Convert.ToInt32(sData[3]);
                        int price = Convert.ToInt32(sData[4]);

                        if (user.GetCoins() >= price)
                        {
                            for (int o = 0; o < amount; o++) user.GiveItem(itemName);
                            user.GiveCoins(-price);
                            items[i] = "";
                            File.WriteAllLines("Files/FreeMarket.txt", items.Where(x => x != ""));

                            string plural = "";
                            if (amount > 1) plural = "s";

                            string pluralC = "";
                            if (price > 1) pluralC = "s";
                            await ReplyAsync($"You have successfully purchased {amount} {itemName}{plural} for {price} coin{pluralC}!");
                            Functions.GetUser(sellerID).GiveCoins(price);
                            await Bot.client.GetUser(Convert.ToUInt64(sellerID)).SendMessageAsync($"{Context.User.Username}#{Context.User.Discriminator} has purchased your {amount} {itemName}{plural} for {price} coin{pluralC}.");
                            break;
                        }
                        else await ReplyAsync(":x: You cannot afford this posting. :x:");
                    }
                }
            }
            else if (command[0] == "cancel")
            {
                var items = File.ReadAllLines("Files/FreeMarket.txt");
                var id = command[1].ToUpper();
                for (int i = 0; i < items.Count(); i++)
                {
                    string[] sData = items[i].Split('|');
                    string sellerID = sData[1];
                    if (items[i].Split('|')[0] == id)
                    {
                        string itemName = sData[2];
                        int amount = Convert.ToInt32(sData[3]);
                
                        if (sellerID == $"{Context.User.Id}")
                        {
                            if (user.GetCoins() >= 200)
                            {
                                user.GiveCoins(-200);
                                items[i] = "";
                                for (int o = 0; o < amount; o++) user.GiveItem(itemName);
                                File.WriteAllLines("Files/FreeMarket.txt", items.Where(x => x != ""));
                                await ReplyAsync($"You have successfully canceled your posting of {amount} {itemName}(s). They have returned to your inventory and you have been charged the cancellation fee of 200 coins.");
                            }
                            else await ReplyAsync("You cannot afford the cancellation fee of 200 coins and have not cancelled this posting.");
                            
                            break;
                        }
                        else
                        {
                            await ReplyAsync(":x: You cannot cancel someone elses posting! :x:");
                        }
                    }
                }
            }
        }

        [Command("iteminfo"), Summary("Its like a pokedex but for items!")]
        public async Task ItemInfo(string item)
        {
            var items = Functions.GetItemList();
            foreach(string i in items)
            {
                if (i.StartsWith(item))
                {
                    var itemInfo = i.Split('|');
                    JEmbed emb = new JEmbed();
                    emb.Title = Functions.GetItemEmote(item) + " " + Func.ToTitleCase(itemInfo[0]).Replace('_',' ');
                    emb.Description = itemInfo[1];
                    emb.ColorStripe = Constants.Colours.YORK_RED;
                    if (itemInfo[2].Contains("-")) emb.Description += $"\n\n:moneybag: Cannot be purchased. Find through presents or combining! Sell: 10 coins.";
                    else emb.Description += $"\n\n:moneybag: Buy: {itemInfo[2]} coins.\nSell: {Convert.ToInt32(Convert.ToInt32(itemInfo[2])* Constants.Values.SELL_VAL)} coins.";
                    await ReplyAsync("", embed: emb.Build());
                    return;
                }
            }
            await ReplyAsync("Item not found.");
        }

        [Command("combine"), Summary("[FUN] Combine lame items to make rad items!")]
        public async Task Combine(params string[] items)
        {
            User u = Functions.GetUser(Context.User);
            
            foreach (string item in items)
            {
                if (!u.GetItemList().Contains(item))
                {
                    await ReplyAsync($"You do not have a(n) {item}!");
                    return;
                }
            }
            

            string result = ItemCombo.CheckCombo(items);
            if (result != null)
            {
                foreach(string item in items) u.RemoveItem(item);
                u.GiveItem(result);
                await ReplyAsync($"You have successfully made a {result}! " + Functions.GetItemEmote(result));
            }
            else
            {
                await ReplyAsync("This combo doesn't exist!");
            }
        }

        [Command("trash"), Summary("[FUN] Throw items away.")]
        public async Task Trash(params string[] items)
        {
            var u = Functions.GetUser(Context.User);
            string msg = "";
            foreach (string item in items)
            {
                if (u.GetItemList().Contains(item))
                {
                    u.RemoveItem(item);
                    msg += ":recycle: You have succesfully thrown away your " + item + "!\n";
                }
                else msg += ":x: You do not have an item called " + item + "!\n";
            }
            var msgs = Functions.SplitMessage(msg);
            foreach (string m in msgs) await ReplyAsync(m);
        }

        #endregion

        #region Fun

        //viewing tag
        [Command("tag"), Summary("Make or view a tag!")]
        public async Task Tag(string tag)
        {
            if (Context.Guild.Id == Constants.Guilds.YORK_UNIVERSITY) return;
            if (!File.Exists("Files/tags.txt")) File.Create("Files/tags.txt");
            string[] tags = File.ReadAllLines("Files/tags.txt");
            bool sent = false;
            string msg = "";
            foreach (string line in tags)
            {
                if (tag == "list")
                {
                    msg += "\n" + line.Split('|')[0];
                }
                else if (line.Split('|')[0] == tag)
                {
                    sent = true;
                    await Context.Channel.SendMessageAsync(line.Split('|')[1]);
                    break;
                }
            }

            if (tag == "list")
            {
                var msgs = Functions.SplitMessage(msg);
                foreach(string message in msgs)
                {
                    await ReplyAsync($"```\n{message}\n```");
                }
            }
            else if (!sent) await Context.Channel.SendMessageAsync("Tag not found!");

        }

        //for creating the tag
        [Command("tag")]
        public async Task Tag(string tag, [Remainder]string content)
        {
            if (Context.Guild.Id == Constants.Guilds.YORK_UNIVERSITY) return;
            if (!File.Exists("Files/tags.txt")) File.Create("Files/tags.txt");
            bool exists = false;
            if (tag == "list") exists = true;
            else if (tag == "delete" && Context.User.Id == Constants.Users.BRADY)
            {
                var tags = File.ReadAllLines("Files/tags.txt").ToList();
                for (int i = 0; i < tags.Count(); i++)
                {
                    if (tags[i].Split('|')[0] == content) { tags.Remove(tags[i]); break; }
                }
                File.WriteAllLines("Files/tags.txt", tags);
            }
            else
            {
                string[] tags = File.ReadAllLines("Files/tags.txt");
                foreach (string line in tags)
                {
                    if (line.Split('|')[0] == tag) { exists = true; break; }
                }

                if (!exists)
                {
                    File.AppendAllText("Files/tags.txt", tag + "|" + content + "\n");
                    await Context.Channel.SendMessageAsync("Tag created!");
                }
                else await Context.Channel.SendMessageAsync("Tag already exists!");
            }
        }
        
        [Command("meme"), Summary("[FUN] Memify STUFF.")]
        public async Task Meme() { await Meme(Context.User); }

        [Command("meme")]
        public async Task Meme(IUser user)
        {
            string path = @"Files\Templates";
            string picURL = user.GetAvatarUrl();
            using (ImageFactory proc = new ImageFactory())
            {
                var imgID = rdm.Next(7) + 1;
                proc.Load(path + $@"\{imgID}.png");
                using (WebClient web = new WebClient())
                {
                    bool downloaded = false;
                    while (!downloaded)
                    {
                        try { web.DownloadFile(picURL, path + @"\0.png"); downloaded = true; }
                        catch (Exception) { }
                    }
                }
                var img = new ImageLayer();
                img.Image = System.Drawing.Image.FromFile(path + @"\0.png");

                string[] texts = { "is a dingus", "is an ape", "is cool", "is dumb", "knows how to hold a fork", "eats poo", "is a wasteman" };

                bool overlay = true;
                switch (imgID)
                {
                    case 1:
                        proc.Load(path + @"\0.png");
                        proc.Resize(new Size(350, 350));
                        img.Image = System.Drawing.Image.FromFile(path + $@"\{imgID}.png");
                        img.Size = new Size(200, 200);
                        img.Position = new Point(75, 0);
                        break;
                    case 2:
                        img.Position = new Point(60, 440);
                        img.Size = new Size(100, 100);
                        break;
                    case 3:
                        img.Position = new Point(335, 190);
                        break;
                    case 4:
                        proc.Resize(new ResizeLayer(size: new Size(600, 600), resizeMode: ResizeMode.Min));
                        overlay = false;
                        var txt = new TextLayer();
                        txt.Text = user.Username + " " + texts[rdm.Next(texts.Count())];
                        txt.FontSize = 50 - (int)(txt.Text.Count() / 1.3);
                        if (txt.FontSize <= 0) txt.FontSize = 1;
                        txt.Position = new Point(20, 630);
                        proc.Watermark(txt);
                        break;
                    case 5:
                        img.Position = new Point(390, 500);
                        break;
                    case 6:
                        var img2 = new ImageLayer();
                        img2.Image = img.Image;
                        var img3 = new ImageLayer();
                        img3.Image = img.Image;
                        img.Position = new Point(60, 100);
                        img.Size = new Size(50, 50);

                        img2.Position = new Point(290, 150);
                        img2.Size = new Size(50, 50);

                        img3.Position = new Point(370, 330);
                        img3.Size = new Size(50, 50);

                        proc.Overlay(img2);
                        proc.Overlay(img3);

                        var text = new TextLayer();

                        text.Text = user.Username + " " + texts[rdm.Next(texts.Count())];

                        char[] cText = text.Text.ToCharArray();
                        int insertCount = 0;
                        for (int i = 7; i < cText.Count(); i++)
                        {
                            if (cText[i] == ' ' && insertCount == 0)
                            {
                                cText[i] = '\n';
                                i += 7;
                            }
                        }

                        for (int i = cText.Count() - 1; i >= 0; i--)
                        {
                            if (cText[i] == ' ')
                            {
                                cText[i] = '\n';
                                break;
                            }
                        }

                        text.Text = new string(cText);
                        text.FontSize = 15;
                        text.Position = new Point(95, 300);
                        proc.Watermark(text);
                        break;
                    case 7:
                        img.Position = new Point(280, 100);
                        img.Size -= new Size(10, 10);
                        break;
                }

                if (overlay) proc.Overlay(img);
                var sImgID = rdm.Next(1000000);
                proc.Save(path + $@"\{sImgID}.png");
                await Context.Channel.SendFileAsync(path + $@"\{sImgID}.png");
                File.Delete(path + $@"\{sImgID}.png");
            }
        }

        [Command("allowance"), Summary("[FUN] Receive your daily free coins.")]
        public async Task Allowance()
        {
            var u = Functions.GetUser(Context.User);
            var lA = u.GetData("allowance");
            DateTime lastAllowance;
            if (lA == "0") lastAllowance = new DateTime(0);
            else lastAllowance = Functions.StringToDateTime(lA);

            var ONE_DAY = new TimeSpan(24, 0, 0);
            if ((lastAllowance + ONE_DAY) < DateTime.Now)
            {
                int allowance = rdm.Next(10, 51);
                u.GiveCoins(allowance);
                u.SetData("allowance", Functions.DateTimeToString(DateTime.Now));
                await Context.Channel.SendMessageAsync($":moneybag: | Here's your daily allowance! ***+{allowance} coins.*** The next one will be available in 24 hours.");
            }
            else
            {
                var next = (lastAllowance + ONE_DAY) - DateTime.Now;
                await Context.Channel.SendMessageAsync($"Your next allowance will be available in {next.Hours} hours, {next.Minutes} minutes, and {next.Seconds} seconds.");
            }
        }

        [Command("hangman"), Summary("[FUN] Play a game of Hangman with the bot."), Alias(new string[] { "hm" })]
        public async Task HangMan()
        {
            if (!Var.hangman)
            {
                var wordList = File.ReadAllLines("Files/wordlist.txt");
                Var.hmWord = wordList[(rdm.Next(wordList.Count()))].ToLower();
                Var.hangman = true;
                Var.hmCount = 0;
                Var.hmErrors = 0;
                Var.guessedChars.Clear();
                await HangMan("");
            }
            else
            {
                await Context.Channel.SendMessageAsync("There is already a game of HangMan running.");
            }
        }

        [Command("hangman"), Alias(new string[] { "hm" })]
        public async Task HangMan(string guess)
        {
            if (Var.hangman)
            {
                guess = guess.ToLower();
                if (guess != "" && Var.guessedChars.Contains(guess[0]) && guess.Count() == 1) await Context.Channel.SendMessageAsync("You've already guessed " + Char.ToUpper(guess[0]));
                else
                {
                    if (guess.Count() == 1 && !Var.guessedChars.Contains(guess[0])) Var.guessedChars.Add(guess[0]);
                    if (guess != "" && ((!Var.hmWord.Contains(guess[0]) && guess.Count() == 1) || (Var.hmWord != guess && guess.Count() > 1))) Var.hmErrors++;


                    string[] hang = {
            "       ______   " ,    //0
            "      /      \\  " ,   //1
            "     |          " ,    //2
            "     |          " ,    //3
            "     |          " ,    //4
            "     |          " ,    //5
            "     |          " ,    //6
            "_____|_____     " };   //7


                    for (int i = 0; i < Var.hmWord.Count(); i++)
                    {
                        if (Var.guessedChars.Contains(Var.hmWord[i])) hang[6] += Char.ToUpper(Convert.ToChar(Var.hmWord[i])) + " ";
                        else hang[6] += "_ ";
                    }

                    for (int i = 0; i < Var.hmErrors; i++)
                    {
                        if (i == 0)
                        {
                            var line = hang[2].ToCharArray();
                            line[13] = 'O';
                            hang[2] = new string(line);
                        }
                        if (i == 1)
                        {
                            var line = hang[3].ToCharArray();
                            line[13] = '|';
                            hang[3] = new string(line);
                        }
                        if (i == 2)
                        {
                            var line = hang[4].ToCharArray();
                            line[12] = '/';
                            hang[4] = new string(line);
                        }
                        if (i == 3)
                        {
                            var line = hang[4].ToCharArray();
                            line[14] = '\\';
                            hang[4] = new string(line);
                        }
                        if (i == 4)
                        {
                            var line = hang[3].ToCharArray();
                            line[12] = '/';
                            hang[3] = new string(line);
                        }
                        if (i == 5)
                        {
                            var line = hang[3].ToCharArray();
                            line[14] = '\\';
                            hang[3] = new string(line);
                        }
                    }

                    if (!hang[6].Contains("_") || Var.hmWord == guess) //win
                    {
                        Var.hangman = false;
                        foreach (char c in Var.hmWord)
                        {
                            Var.guessedChars.Add(c);
                        }
                        var u = Functions.GetUser(Context.User);
                        int coinReward = rdm.Next(40)+10;
                        u.GiveCoins(coinReward);
                        await Context.Channel.SendMessageAsync($"You did it! You got {coinReward} coins.");
                    }

                    hang[6] = "     |          ";
                    for (int i = 0; i < Var.hmWord.Count(); i++)
                    {
                        if (Var.guessedChars.Contains(Var.hmWord[i])) hang[6] += Char.ToUpper(Convert.ToChar(Var.hmWord[i])) + " ";
                        else hang[6] += "_ ";
                    }

                    if (Var.hmErrors == 6)
                    {
                        await Context.Channel.SendMessageAsync("You lose! The word was: " + Var.hmWord);
                        Var.hangman = false;
                    }

                    string msg = "```\n";
                    foreach (String s in hang) msg += s + "\n";
                    msg += "```";
                    if (Var.hangman)
                    {
                        msg += "Guessed letters: ";
                        foreach (char c in Var.guessedChars) msg += char.ToUpper(c) + " ";
                        msg += "\nUse `;hangman [guess]` to guess a character or the entire word.";

                    }
                    await Context.Channel.SendMessageAsync(msg);
                }
            }
            else
            {
                await HangMan();
                await HangMan(guess);
            }
        }

        [Command("profile"), Summary("View your or another users profile.")]
        public async Task Profile([Remainder] IUser user)
        {
            var u = Functions.GetUser(user);

            var emb = new JEmbed();
            emb.Author.Name = user.Username;
            emb.Author.IconUrl = user.GetAvatarUrl();

            emb.ColorStripe = Functions.GetColor(user);

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Coins:";
                x.Text = u.GetCoins().ToString();
                x.Inline = true;
            }));


            var gUser = (user as IGuildUser);
            if (gUser != null)
            {
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Roles:";
                    string text = "";

                    foreach (ulong id in gUser.RoleIds)
                    {
                        text += Context.Guild.GetRole(id).Name + "\n";
                    }

                    x.Text = Convert.ToString(text);
                    x.Inline = true;
                }));
            }

            bool moreItems = true;
            string[] items = u.GetItemList();
            string invTitle = "Inventory";
            while (moreItems)
            {
                List<string> extraItems = new List<string>();
                moreItems = false;
                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = invTitle + ":";
                    string text = "";
                    foreach (string item in items)
                    {
                        if (moreItems) extraItems.Add(item);
                        else
                        {
                            if (text.Length + (item + ", ").Length > 1024)
                            {
                                moreItems = true;
                                extraItems.Add(item);
                                invTitle += " (cont)";
                            }
                            else text += item + ", ";
                        }
                    }
                    x.Text = Convert.ToString(text);
                    if (moreItems) items = extraItems.ToArray();
                }));
            }

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Stats:";
                string text = "";
                foreach (string stat in u.GetStats())
                {
                    var s = stat.Replace("stat.", "");
                    var info = s.Split(':');
                    text += info[0] + ": " + info[1] + "\n";
                }
                x.Text = Convert.ToString(text);
            }));

            await Context.Channel.SendMessageAsync("", embed: emb.Build());
        }

        [Command("profile")]
        public async Task Profile() => await Profile(Context.User);

        [Command("present"), Summary("[FUN] Get a cool gift!")]
        public async Task Present()
        {
            if (!Var.presentWaiting)
            {
                if (Var.presentTime < Var.CurrentDate() - Var.presentWait)
                {
                    Var.presentCount = rdm.Next(4) + 1;
                    Var.presentClaims.Clear();
                }

                if (Var.presentCount > 0 && !Var.presentClaims.Any(x => x.Id == Context.User.Id))
                {
                    if (Var.presentClaims.Count() <= 0)
                    {
                        Var.presentWait = new TimeSpan(rdm.Next(4), rdm.Next(60), rdm.Next(60));
                        Var.presentTime = Var.CurrentDate();
                    }
                    Var.presentCount--;
                    Var.presentClaims.Add(Context.User as IGuildUser);
                    Var.presentNum = rdm.Next(10);
                    if (!Var.presentRigged)
                    {
                        await Context.Channel.SendMessageAsync($"A present appears! :gift: Press {Var.presentNum} to open it!");
                        Var.presentWaiting = true;
                        Var.replacing = false;
                        Var.replaceable = true;
                    }
                    else
                    {
                        Var.presentRigged = false;
                        User user = Functions.GetUser(Context.User);

                        if (user.GetData("gnoming") == "1")
                        {
                            user.SetData("gnoming", "0");
                            await ReplyAsync(Functions.GetItemEmote("gnome") + $" Whoa! The present was rigged by {Var.presentRigger.Mention} [{Var.presentRigger.Username}]! Your gnome sacrificed himself to save your items!\n{Constants.Values.GNOME_VID}");
                            await Context.Channel.SendMessageAsync($"A present appears! :gift: Press {Var.presentNum} to open it!");
                            Var.presentWaiting = true;
                            Var.replacing = false;
                            Var.replaceable = true;
                        }
                        else
                        {
                            int lossCount = rdm.Next(5) + 1;
                            if (lossCount > user.GetItemList().Count()) lossCount = user.GetItemList().Count();
                            if (lossCount == 0)
                            {
                                await ReplyAsync($":bomb: Oh no! The present was rigged by {Var.presentRigger.Mention} [{Var.presentRigger.Username}] and you lost... Nothing??\n:boom::boom::boom::boom:");
                            }
                            else
                            {
                                string msg = $":bomb: Oh no! The present was rigged by {Var.presentRigger.Mention} and you lost:\n```";
                                for (int i = 0; i < lossCount; i++)
                                {
                                    string item = user.GetItemList()[rdm.Next(user.GetItemList().Count())];
                                    user.RemoveItem(item);
                                    msg += item + "\n";
                                }
                                await ReplyAsync(msg + "```\n:boom::boom::boom::boom:");
                            }
                        }
                    }


                }
                else
                {
                    var timeLeft = Var.presentTime - (Var.CurrentDate() - Var.presentWait);
                    var msg = $"The next presents are not available yet! Please be patient! They should be ready in *about* {timeLeft.Hours + 1} hour(s)!";
                    if (Var.presentClaims.Count() > Properties.Settings.Default.recordClaims) { Properties.Settings.Default.recordClaims = Var.presentClaims.Count(); Properties.Settings.Default.Save(); }
                    msg += $"\nThere have been {Var.presentClaims.Count()} claims! The record is {Properties.Settings.Default.recordClaims}.";
                    if (Var.presentClaims.Count() > 0)
                    {
                        msg += "\nLast claimed by:\n```\n";
                        foreach (IGuildUser user in Var.presentClaims) msg += $"\n{user.Username} in {user.Guild}";
                        msg += "\n```";
                    }
                    msg += $"There are {Var.presentCount} presents left!";
                    await Context.Channel.SendMessageAsync(msg);
                }
            }
        }

        [Command("poll"), Summary("[FUN] Create a poll for users to vote on.")]
        public async Task Poll(string command = "", params string[] parameters)
        {
            if (command == "")
            {
                if (Var.currentPoll != null && !Var.currentPoll.completed) await Context.Channel.SendMessageAsync("", embed: Var.currentPoll.GenerateEmbed());
                else await Context.Channel.SendMessageAsync("There is currently no poll. Create one with `;poll create [question] [option1] [option2] etc..`");
            }
            else if (command == "create")
            {
                if (Var.currentPoll == null || Var.currentPoll.completed)
                {
                    Var.currentPoll = new Poll(Context.Channel, 5, parameters[0], parameters.Where(x => x != parameters[0]).ToArray());
                    await Context.Channel.SendMessageAsync("Poll created!", embed: Var.currentPoll.GenerateEmbed());
                }
                else await Context.Channel.SendMessageAsync("There is already a poll running.");
            }
            else if (command == "vote")
            {
                if (Var.currentPoll != null && !Var.currentPoll.completed)
                {
                    if (!Var.currentPoll.HasVoted(Context.User.Id))
                    {
                        Var.currentPoll.Vote(Char.Parse(parameters[0]));
                        Var.currentPoll.voted.Add(Context.User.Id);
                        await Context.Channel.SendMessageAsync("You have successfully voted for option " + parameters[0] + ".");
                    }
                    else await Context.Channel.SendMessageAsync("You have already voted!");
                }
                else await Context.Channel.SendMessageAsync("There is currently no poll.");
            }
        }

        [Command("top"), Summary("[FUN] View the top users of ForkBot based on their stats. Use a stat as the parameter in order to see specific stat rankings.")]
        public async Task Top(string stat = "")
        {
            var top5 = Functions.GetTopList(stat);
            string msg = "```\nTop five users";
            if (stat == "bottom")
            {
                msg = msg.Replace("Top", "Bottom");
                stat = "";
            }
            else if (stat != "") msg += " [" + stat + "]";
            msg += ":\n";
            
            int amount = 5;
            if (top5.Count() < 5) amount = top5.Count();
            for (int i = 0; i < amount; i++)
            {
                var user = Bot.client.GetUser(top5[i].Key);
                string userName;
                if (user == null) userName = $"User[{top5[i].Key}]";
                else userName = user.Username;
                msg += $"[{i + 1}] {userName} - {top5[i].Value} {stat}\n";
            }
            msg += "```";
            await Context.Channel.SendMessageAsync(msg);
        }
        
        [Command("lottery"), Summary("[FUN] The Happy Lucky Lottery! Buy a lotto card and check daily to see if your numbers match!")]
        public async Task Lottery(string command = "")
        {
            User u = Functions.GetUser(Context.User);

            if (command == "")
            {
                var currentDay = Var.CurrentDate();
                if (Var.lottoDay.DayOfYear < currentDay.DayOfYear || Var.lottoDay.Year < currentDay.DayOfYear)
                {
                    Var.lottoDay = Var.CurrentDate();
                    Var.todaysLotto = $"{rdm.Next(10)}{rdm.Next(10)}{rdm.Next(10)}{rdm.Next(10)}";
                }

                JEmbed emb = new JEmbed();
                emb.Title = "Happy Lucky Lottery";
                emb.Description = "It's the Happy Lucky Lottery!\nMatch any of todays digits with your number to win prizes!\n\n" +
                                  "Todays number: " + Var.todaysLotto;
                emb.ColorStripe = Functions.GetColor(Context.User);

                string uNum = u.GetData("lotto");
                if (uNum == "0") emb.Footer.Text = "Get your number today with ';lottery buy'!";
                else
                {
                    var lottoDay = Functions.StringToDateTime(u.GetData("lottoDay"));
                    if (lottoDay.DayOfYear >= Var.CurrentDate().DayOfYear && lottoDay.Year == Var.CurrentDate().Year) await ReplyAsync("You've already checked the lottery today! Come back tomorrow!");
                    else
                    {
                        u.SetData("lottoDay", Functions.DateTimeToString(Var.CurrentDate()));
                        int matchCount = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            if (uNum[i] == Var.todaysLotto[i]) matchCount++;
                        }

                        emb.Fields.Add(new JEmbedField(x =>
                        {
                            x.Header = "Matches";
                            x.Text = $"You got {matchCount} match(es)!";
                            if (matchCount == 0) x.Text += "\nSorry!";
                            else
                            {
                                x.Text += "\nCongratulations! ";
                                switch (matchCount)
                                {
                                    case 1:
                                        x.Text += "You got 100 coins!";
                                        u.GiveCoins(100);
                                        break;
                                    case 2:
                                        string[] level2Items = { "baby", "8ball", "paintbrush", "game_die", "watch", "gift" };
                                        string item = level2Items[rdm.Next(level2Items.Count())];
                                        x.Text += $"You got 500 coins and a(n) {item} {Functions.GetItemEmote(item)}!";
                                        u.GiveCoins(500);
                                        u.GiveItem(item);
                                        break;
                                    case 3:
                                        string[] level3Items = { "key", "key", "slot_machine", "gun", "unicorn", "moneybag", "moneybag", "ticket" };
                                        var item01 = level3Items[rdm.Next(level3Items.Count())];
                                        var item02 = level3Items[rdm.Next(level3Items.Count())];
                                        x.Text += $"You got 1000 coins and: {item01} {Functions.GetItemEmote(item01)}, {item02} {Functions.GetItemEmote(item02)}";
                                        u.GiveCoins(1000);
                                        u.GiveItem(item01);
                                        u.GiveItem(item02);
                                        break;
                                    case 4:
                                        string[] level4Items = { "key", "key", "slot_machine", "gun", "unicorn", "moneybag", "moneybag", "ticket" };
                                        var item1 = level4Items[rdm.Next(level4Items.Count())];
                                        var item2 = level4Items[rdm.Next(level4Items.Count())];
                                        var item3 = level4Items[rdm.Next(level4Items.Count())];
                                        x.Text += $"You got 5000 coins and: {item1} {Functions.GetItemEmote(item1)}, {item2} {Functions.GetItemEmote(item2)}, {item3} {Functions.GetItemEmote(item3)}";
                                        u.GiveCoins(5000);
                                        u.GiveItem(item1);
                                        u.GiveItem(item2);
                                        u.GiveItem(item3);
                                        break;
                                }
                            }
                        }));
                    }
                    emb.Footer.Text = $"Your number: {uNum}";
                }


                await ReplyAsync("", embed: emb.Build());
            }
            else if (command == "buy")
            {
                string uNum = u.GetData("lotto");
                if (uNum == "0") await ReplyAsync("Are you sure you want to buy a lottery ticket for 10 coins? Use `;lottery confirm` to confirm!");
                else await ReplyAsync("Are you sure you want to buy a *new* lottery ticket for 100 coins? Use `;lottery confirm` to confirm!");
            }
            else if (command == "confirm")
            {
                string uNum = u.GetData("lotto");
                int cost = 0;
                if (uNum == "0") cost = 10;
                else cost = 100;
                
                if (u.GetCoins() >= cost)
                {
                    u.GiveCoins(-cost);
                    u.SetData("lotto", $"{rdm.Next(10)}{rdm.Next(10)}{rdm.Next(10)}{rdm.Next(10)}");
                    await ReplyAsync($"You have successfully purchased a Happy Lucky Lottery Ticket for {cost} coins!");
                }
                else
                {
                    await ReplyAsync($"You cannot afford a ticket! You need {cost} coins.");
                }
            }
        }

        [Command("tip"), Summary("[FUN] Get a random ForkBot tip, or use a number as the parameter to get a specific tip!")]
        public async Task Tip(int tipNumber = -1)
        {
            string[] tips = { "Use `;makekey` to combine 5 packages into a key!",
                "Get presents occasionally with `;present'! No presents left? Use a ticket to add more to the batch, or a stopwatch to shorten the time until the next batch!",
                "Get coins for items you don't need or want by selling them with `;sell`! Item can't be sold? Just `;trash` it!",
                "Give other users coins with the `;donate` command!",
                "Legend says of a secret shop that only the most elite may enter! I think the **man** knows..."
                };
            if (tipNumber == -1) tipNumber = rdm.Next(tips.Count());
            else tipNumber--;

            if (tipNumber < 0 || tipNumber > tips.Count()) await ReplyAsync($"Invalid tip number! Make sure number is above 0 and less than {tips.Count() + 1}");
            await ReplyAsync($":robot::speech_balloon: " + tips[tipNumber]);
        }

        [Command("minesweeper"), Summary("[FUN] Play a game of MineSweeper and earn coins!"), Alias(new string[] {"ms"})]
        public async Task MineSweeper([Remainder]string command = "")
        {

            MineSweeper game = Var.MSGames.Where(x => x.player.ID == Context.User.Id).FirstOrDefault();
            if (command == "" && game == null)
            {
                game = new MineSweeper(Functions.GetUser(Context.User));
                await ReplyAsync(game.Build());
                Var.MSGames.Add(game);
                await ReplyAsync("Use `;ms x,y` (replacing x and y with letter coordinates) to reveal a tile, or `;ms flag x,y` to flag a tile.");
            }
            else if (command.ToLower().StartsWith("flag") && game != null)
            {
                var coords = command.ToLower().Split(' ')[1].Split(',');
                var success = game.Flag(coords);
                if (success) await ReplyAsync(game.Build());
                else await ReplyAsync("Make sure the tile you choose is unrevealed.");
            }
            else
            {
                var coords = command.ToLower().Split(',');
                var success = game.Turn(coords);
                if (success) await ReplyAsync(game.Build());
                else await ReplyAsync("Make sure the tile you choose is unrevealed.");
            }
        }

        [Command("valentine"), Summary("[FUN] Send a Valentine to your love!")]
        public async Task Valentine(IUser user)
        {
            string path = @"Files\VTemplates";
            var imgs = Directory.GetFiles(path);
            using (ImageFactory proc = new ImageFactory())
            {

                var imgID = rdm.Next(imgs.Count());
                proc.Load($@"{imgs[imgID]}");

                TextLayer sender = new TextLayer();
                TextLayer reciever = new TextLayer();

                sender.Text = Functions.GetName(Context.User as IGuildUser);
                reciever.Text = Functions.GetName(user as IGuildUser);

                int fontsize = 20;
                
                switch (imgID)
                {
                    case 0:
                        reciever.Position = new Point(70, 170);
                        sender.Position = new Point(70, 205);
                        break;
                    case 1:
                        reciever.Position = new Point(130, 210);
                        sender.Position = new Point(130, 250);
                        break;
                    case 2:
                        fontsize = 50;
                        reciever.Position = new Point(130, 330);
                        sender.Position = new Point(130, 465);
                        break;
                    case 3:
                        fontsize = 15;
                        reciever.Position = new Point(140, 25);
                        sender.Position = new Point(140, 55);
                        break;
                    case 4:
                        reciever.Position = new Point(300, 190);
                        sender.Position = new Point(330, 250);
                        break;
                    case 5:
                        fontsize = 15;
                        reciever.Position = new Point(50,110);
                        sender.Position = new Point(50, 140);
                        break;
                    case 6:
                        reciever.Position = new Point(120, 140);
                        sender.Position = new Point(130, 190);
                        break;
                    case 7:
                        reciever.Position = new Point(530, 320);
                        sender.Position = new Point(560, 360);
                        break;
                    case 8:
                        reciever.Position = new Point(300, 150);
                        sender.Position = new Point(300, 200);
                        break;
                    case 9:
                        sender.Position = new Point(380, 210);
                        reciever.Position = new Point(340, 175);
                        break;
                    case 10:
                        reciever.Position = new Point(320, 270);
                        sender.Position = new Point(350, 310);
                        break;
                    case 11:
                        sender.Position = new Point(210, 305);
                        reciever.Position = new Point(180, 230);
                        break;
                    case 12:
                        sender.Position = new Point(450,310);
                        reciever.Position = new Point(400, 250);
                        break;
                    case 13:
                        sender.Position = new Point(360, 250);
                        reciever.Position = new Point(340, 215);
                        break;
                    case 14:
                        sender.Position = new Point(70, 300);
                        reciever.Position = new Point(70, 230);
                        break;
                    case 15:
                        sender.Position = new Point(430, 100);
                        reciever.Position = new Point(400,60);
                        break;
                    case 16:
                        sender.Position = new Point(285, 220);
                        reciever.Position = new Point(260, 190);
                        break;

                }
                sender.FontSize = fontsize;
                reciever.FontSize = fontsize;
                proc.Watermark(sender);
                proc.Watermark(reciever);
                var sImgID = rdm.Next(1000000);
                proc.Save(path + $@"\{sImgID}.png");
                await Context.Channel.SendFileAsync(path + $@"\{sImgID}.png");
                File.Delete(path + $@"\{sImgID}.png");
            }
        }
        
        [Command("forkparty"), Summary("[FUN] Begin a game of ForkParty:tm: with up to 4 players!"), Alias(new string[] { "fp" })]
        public async Task ForkParty([Remainder] string command = "")
        {
            var user = Functions.GetUser(Context.User);
            var chanGames = Var.FPGames.Where(x => x.Channel.Id == Context.Channel.Id);
            ForkParty game = null;
            if (chanGames.Count() != 0) game = chanGames.First();
            if (command == "")
            {
                string msg = "Welcome to ForkParty:tm:! This is a Mario Party styled game in which players move around a board and play minigames to collect the most Forks and win!\n" +
                                "Use `;fp host` to start a game, or `;fp join` in the hosts channel to join a game.";

                if (game != null)
                {
                    msg += $"\n```\nThere is currently a game being hosted by {await game.Players[0].GetName(Context.Guild)}. There is {4 - game.PlayerCount} spot(s) left.\n";
                    for (int i = 1; i < game.PlayerCount; i++) msg += await game.Players[i].GetName(Context.Guild) + "\n";
                    for (int i = 0; i < 4 - game.PlayerCount; i++) msg += "----------\n";
                    msg += "```";
                }

                await ReplyAsync(msg);
            }
            else if (command == "host")
            {
                if (game != null)
                {
                    string msg = "There is already a ForkParty game being hosted in this channel.";
                    if (game.Started) msg += " It has not started yet, join with `;fp join`!";
                    await ReplyAsync(msg);
                }
                else
                {
                    Var.FPGames.Add(new ForkParty(user, Context.Channel));
                    await ReplyAsync("You have successfully hosted a game. Get others to join now!");
                }
                
            }
            else if (command == "join")
            {
                if (game != null)
                {
                    if (!game.Started)
                    {
                        if (game.PlayerCount < 4)
                        {
                            if (!game.HasPlayer(user))
                            {
                                game.Join(user);
                                await ReplyAsync($"You have successfully joined {await game.Players[0].GetName(Context.Guild)}'s game.");
                            }
                            else
                                await ReplyAsync("You have already joined this game.");
                        }
                        else await ReplyAsync("There are no spaces remaining in the game being hosted in this channel.");
                    }
                    else await ReplyAsync("There is already game started in this channel. Wait for it to end or find another!");
                }
                else await ReplyAsync("There is currently no game being hosted in this channel. Host a game with `;fp host`!");
            }
        }
        
        [Command("raid"), Summary("[FUN] Choose a class then take on enemies to level up and gain glorious loot!")]
        public async Task RaidCommand([Remainder] string command = "")
        {
            //throw new NotImplementedException("Not finished coding.");
            
            var user = Functions.GetUser(Context.User);
            if (user.GetData("raid.class") == "0")
            {
                await ReplyAsync(Raid.startMessage);
            }
            
        }
        #endregion

        #region Mod Commands

        [Command("ban"),RequireUserPermission(GuildPermission.BanMembers), Summary("[MOD] Bans the specified user.")]
        public async Task Ban(IGuildUser u, int minutes = 0, [Remainder]string reason = null)
        {
            string rText = ".";
            if (reason != null) rText = $" for: \"{reason}\".";
            InfoEmbed banEmb = new InfoEmbed("USER BAN", $"User: {u} has been banned{rText}.", Constants.Images.Ban);
            await Context.Guild.AddBanAsync(u, reason: reason);
            await Context.Channel.SendMessageAsync("", embed: banEmb.Build());
        }

        [Command("kick"), RequireUserPermission(GuildPermission.KickMembers), Summary("[MOD] Kicks the specified user.")]
        public async Task Kick(IUser u, [Remainder]string reason = null)
        {
            string rText = ".";
            if (reason != null) rText = $" for: \"{reason}\".";
            InfoEmbed kickEmb = new InfoEmbed("USER KICK", $"User: {u} has been kicked{rText}", Constants.Images.Kick);
            await (u as IGuildUser).KickAsync(reason);
            await Context.Channel.SendMessageAsync("", embed: kickEmb.Build());
        }

        [Command("purge"), RequireUserPermission(GuildPermission.ManageMessages), Summary("[MOD] Delete [amount] messages")]
        public async Task Purge(int amount)
        {
            Var.purging = true;
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);

            InfoEmbed ie = new InfoEmbed("PURGE", $"{amount} messages deleted by {Context.User.Username}.");
            Var.purgeMessage = await Context.Channel.SendMessageAsync("", embed: ie.Build());
            Timers.unpurge = new Timer(new TimerCallback(Timers.UnPurge), null, 5000, Timeout.Infinite);
        }

        [Command("block"), Summary("[MOD] Temporarily stops users from being able to use the bot.")]
        public async Task Block(IUser u)
        {
            if (Context.User.Id != Constants.Users.BRADY) { await ReplyAsync("This can only be used by the bot owner."); return; }
            if (Var.blockedUsers.Contains(u)) Var.blockedUsers.Remove(u);
            else Var.blockedUsers.Add(u);
            
        }
        
        [Command("blockword"), RequireUserPermission(GuildPermission.ManageMessages), Summary("[MOD] Adds the inputted word to the word filter.")]
        public async Task BlockWord([Remainder] string word)
        {
            if (Context.Guild.Id != Constants.Guilds.YORK_UNIVERSITY) { await ReplyAsync("This can only be used in the York University server."); return; }
            Properties.Settings.Default.blockedWords += word + "|";
            Properties.Settings.Default.Save();
            await ReplyAsync("", embed: new InfoEmbed("Word Blocked", "Word successfully added to filter.").Build());
            await Context.Message.DeleteAsync();
        }

        [Command("trust"), RequireUserPermission(GuildPermission.MoveMembers), Summary("[MOD] Makes a user trusted.")]
        public async Task Trust(IGuildUser user)
        {
            if (Context.Guild.Id != Constants.Guilds.YORK_UNIVERSITY) { await ReplyAsync("This command is only for the York University server."); return; }
            var u = Functions.GetUser(user);
            u.SetData("isTrusted", "true");
            u.SetData("lastInfraction", "0");
            await user.AddRoleAsync(user.Guild.GetRole(Constants.Roles.TRUSTED));
            await ReplyAsync($"Trusted {Functions.GetName(user)}.");
        }

        [Command("record")]
        public async Task Record() => await Record(Context.User as IGuildUser);

        [Command("record"), Summary("Shows a users progress to being auto trusted.")]
        public async Task Record(IGuildUser user)
        {
            var u = Functions.GetUser(user);
            var joinDate = user.JoinedAt.GetValueOrDefault();
            var lastInfraction = u.GetData("lastInfraction");
            var messageCount = u.GetData("messages");
            var isTrusted = u.GetData("isTrusted");
            var tMsgs = u.GetData("trustedMsgs");

            JEmbed emb = new JEmbed();
            emb.Author.Name = user.Username;
            emb.Author.IconUrl = user.GetAvatarUrl();

            if (isTrusted == "true")
            {
                emb.ColorStripe = Discord.Color.Green;
                emb.Title = $"{user.Username} is Trusted!";
            }
            else
            {
                emb.ColorStripe = Discord.Color.Red;
                emb.Title = $"{user.Username}'s Progress to Being Trusted";
            }

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Message Count";
                if (isTrusted == "true" || tMsgs == "true") messageCount = "500";
                x.Text = $"{messageCount}/500";
                x.Inline = true;
            }));

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Last Infraction";

                if (lastInfraction != "0")
                {
                    var iDate = Functions.StringToDateTime(lastInfraction);
                    var time = Var.CurrentDate() - iDate;
                    x.Text = $"{time.Days} Days, {time.Hours} Hours, and {time.Minutes} minutes.";
                }
                else x.Text = "Clean slate!";
                x.Inline = true;
            }));

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Join Date";
                x.Text = $"{joinDate.Day}/{joinDate.Month}/{joinDate.Year} {joinDate.Hour-4}:{joinDate.Minute}";
                x.Inline = true;
            }));

            await ReplyAsync("", embed: emb.Build());

        }
        #endregion

        #region Brady Commands

        [Command("todo"), Summary("[BRADY] View, add, and remove reminders.")]
        public async Task Todo([Remainder] string reminder = "")
        {
            if (reminder != "")
            {
                if (Context.User.Id == Constants.Users.BRADY)
                {
                    if (reminder.Trim().StartsWith("-"))
                    {
                        bool removed = false;
                        List<string> reminders = File.ReadAllLines("Files/reminders.txt").ToList();
                        for (int i = 0; i < reminders.Count(); i++)
                        {
                            if (reminders[i].ToLower().StartsWith(reminder.Trim().Replace("-", "").Trim().ToLower()))
                            {
                                reminders.RemoveAt(i);
                                File.WriteAllLines("Files/reminders.txt", reminders);
                                removed = true;
                                break;
                            }
                        }
                        if (removed) await Context.Channel.SendMessageAsync("Successfully removed reminder.");
                        else await Context.Channel.SendMessageAsync("Specified reminder not found.");
                    }
                    else
                    {
                        File.AppendAllText("Files/reminders.txt", reminder + "\n");
                        await Context.Channel.SendMessageAsync("Added");
                    }
                }
            }
            else
            {
                string reminders = File.ReadAllText("Files/reminders.txt");
                await Context.Channel.SendMessageAsync(reminders);
            }
        }
        
        [Command("givecoins"), Summary("[BRADY] Give a user [amount] coins.")]
        public async Task Give(IUser user, int amount)
        {
            if (Context.User.Id == Constants.Users.BRADY)
            {
                User u = Functions.GetUser(user);
                u.GiveCoins(amount);
                await Context.Channel.SendMessageAsync($"{user.Username} has successfully been given {amount} coins.");
            }
            else await Context.Channel.SendMessageAsync("Sorry, only Brady can use this right now.");
        }

        [Command("giveitem"), Summary("[BRADY] Give a user [item] item")]
        public async Task Give(IUser user, string item)
        {
            if (Context.User.Id == Constants.Users.BRADY)
            {
                User u = Functions.GetUser(user);
                u.GiveItem(item);
                await Context.Channel.SendMessageAsync($"{user.Username} has successfully been given: {item}.");
            }
            else await Context.Channel.SendMessageAsync("Sorry, only Brady can use this right now.");
        }

        [Command("eval"), Summary("[BRADY] Evaluates inputted C# code.")]
        public async Task EvaluateCmd([Remainder] string expression)
        {
            if (Context.User.Id == Constants.Users.BRADY)
            {
                IUserMessage msg = await ReplyAsync("Evaluating...");
                string result = await EvalService.EvaluateAsync(Context as CommandContext, expression);
                var user = Context.User as IGuildUser;
                var emb = new EmbedBuilder().WithColor(Functions.GetColor(Context.User)).WithDescription(result).WithTitle("Evaluated").WithCurrentTimestamp();
                await Context.Channel.SendMessageAsync("", embed: emb);
            }

        }

        [Command("respond"), Summary("[BRADY] Toggles whether the bot should listen or respond to messages with AI")]
        public async Task Respond()
        {
            if (Context.User.Id == Constants.Users.BRADY)
            {
                Var.responding = !Var.responding;
                if (Var.responding) await Context.Channel.SendMessageAsync("Responding");
                else await Context.Channel.SendMessageAsync("Listening");
            }
        }

        [Command("sblock"), Summary("[BRADY] Blocks specified [user] from giving suggestions.")]
        public async Task SuggestionBlock(IUser user)
        {
            if (Context.User.Id != Constants.Users.BRADY) throw NotBradyException;
            Properties.Settings.Default.sBlocked.Add(Convert.ToString(user.Id));
            Properties.Settings.Default.Save();
            await Context.Channel.SendMessageAsync("Blocked");
        }
        
        [Command("addcourse"), Summary("[BRADY] Adds the specified course to the course list.")]
        public async Task AddCourse([Remainder]string course = "")
        {
            if (Context.User.Id != Constants.Users.BRADY) throw NotBradyException;
            if (course != "")
            {
                course.Replace("\\t", "\t");
                File.AppendAllText("Files/courselist.txt", "\n" + course);
                await ReplyAsync("Successfully added course.");
            }
            else await ReplyAsync("FORMAT EXAMPLE: `LE/EECS 4404 3.00\tIntroduction to Machine Learning and Pattern Recognition`");
        }
        
        [Command("fban"), Summary("[BRADY] Pretend to ban someone hahahahaha..")]
        public async Task FBan(string user)
        {
            if (Context.User.Id != Constants.Users.BRADY) throw NotBradyException;
            await Context.Message.DeleteAsync();
            await ReplyAsync($"{user} has left the server.");
        }

        [Command("giveallitem"), Summary("[BRADY] Give all users an item and optionally display a message.")]
        public async Task GiveAllItem(string item, [Remainder] string msg = "")
        {
            var users = Directory.GetFiles("Users");
            foreach (string u in users)
            {
                var uID = u.Replace(".user", "").Split('\\')[1];
                try
                {
                    var user = Bot.client.GetUser(Convert.ToUInt64(uID));
                    Functions.GetUser(user).GiveItem(item);
                }
                catch (Exception) { Console.WriteLine($"Unable to give user ({u}) item."); }
            }
            if (msg != "") await ReplyAsync("", embed: new InfoEmbed("", msg += $"\nEveryone has recieved a(n) {item}!", Constants.Images.ForkBot).Build());
        }

        [Command("courses")]
        public async Task Courses() { await Courses("",""); }
        
        [Command("courses"), Summary("[BRADY] Return all courses with certain parameters. Leave parameters blank for examples. THIS COMMAND MAY TAKE A LONG TIME.")]
        public async Task Courses(string subject, [Remainder] string commands)
        {
            if (Context.User.Id != Constants.Users.BRADY)
            {
                await ReplyAsync("Sorry, only Brady can use this right now. I'm testing it.");
                return;
            }

            if (commands == "")
            {
                string msg = "This command generates a list of courses that fall under the chosen parameters in order to help find courses with certain criteria easier.\n\n" +
                             "`;courses EECS day:MTWR term:W` Shows EECS courses with classes from Monday to Thursday (Thursday is an R to reflect York website) during the **W**inter term.\n\n" +
                             "`;courses ITEC time>16 day:MWF` Shows ITEC courses with classes after 4pm (24 hour clock) and on Monday, Wednesday, or Friday.\n\n" +
                             "Time can be structed as `time>#`, or 'time<#' for classes after a certain time and classes before a certain time.\n\n" +
                             "`;courses MATH credit:3 level:3???` Shows MATH courses with 3 credits in the 3000 level. For levels, ?'s are wildcards and can be replaced with any number.\n\n" +
                             "Parameters dont need to be in any specific order, just seperated by spaces. However, the subject (MATH, EECS, ITEC, etc) must be first.";
                await ReplyAsync("", embed: new InfoEmbed(";courses EXAMPLES", msg).Build());
                return;
            }

            if (commands.Split(' ').Count()  > 4)
            {
                await ReplyAsync("Over max parameter count. Are you sure all parameters are correct and you are not using the same one multiple times?");
                return;
            }

            await ReplyAsync("Filtering... Please wait...");

            commands = commands.Replace("time>", "time>:").Replace("time<", "time<:");

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (string command in commands.Split(' '))
            {
                if (!command.Contains(':') && command.Length > 1) await ReplyAsync($"Parameter '{command}' invalid. Continuing without this parameter.");
                else if (!command.Contains(':'))
                {
                    await ReplyAsync($"Parameter '{command}' invalid. Cancelling.");
                    return;
                }
                else
                {
                    string[] cParams = command.Split(':');
                    var type = cParams[0];
                    var cond = cParams[1];
                    parameters.Add(type, cond);
                }
            }


            string[] courses = File.ReadAllLines("Files/courselist.txt");
            List<string> courselist = new List<string>();
            foreach (string course in courses)
            {
                var data = course.Split('/');
                if (data.Count() > 1)
                    if (data[1].StartsWith(subject.ToUpper())) courselist.Add(course);
            }

            List<string> filteredCourseList = new List<string>();

            foreach(string course in courselist)
            {
                bool qualifies = true;
                Course c = new Course();
                try
                {
                    c.LoadCourse(course);
                }
                catch (Exception)
                {
                    Console.WriteLine($"Failed to load course page for {course}");
                    qualifies = false;
                    continue;
                }
                var courseDays = c.GetSchedule().Days;

                foreach (KeyValuePair<string,string> param in parameters)
                {
                    switch(param.Key)
                    {
                        //credit level day time 
                        case "day":
                            qualifies = false;
                            Dictionary<char, string> DayConversion = new Dictionary<char, string>() { { 'M', "Monday" }, { 'T', "Tuesday" }, { 'W', "Wednesday" }, { 'R', "Thursday" }, { 'F', "Friday" } };
                            string days = param.Value.ToUpper();
                            var filterDays = days.Select(x => DayConversion[x]);

                            foreach(CourseDay day in courseDays)
                            {
                                foreach (string d in filterDays)
                                {
                                    if (day.DayTimes.ContainsKey(d))
                                    {
                                        qualifies = true;
                                        break;
                                    }
                                }
                                if (qualifies) break;
                            }                            
                            break;

                        case "time":


                            break;

                        case "credit":

                            if (c.GetCredit() != Convert.ToDouble(param.Value)) qualifies = false;
                            break;

                        case "level":

                            string fLevel = param.Value.ToUpper(); //filter level
                            string level = c.GetCode();
                            for (int x = 0; x < 4; x++) if ((fLevel[x] != '?' && fLevel[x] != level[x])) { qualifies = false; break; }
                            break;

                        default:

                            await ReplyAsync($"Parameter '{param.Key}' invalid. Continuing without this parameter.");
                            break;

                    }
                    if (!qualifies) break;
                }

                if (qualifies) filteredCourseList.Add(course);


            }

            string text = "";
            foreach (string course in filteredCourseList) text += course + "\n";

            JEmbed emb = new JEmbed();
            emb.Title = $"Filtered course list: [{subject} {commands}]";
            emb.Description = text;
            emb.ColorStripe = Constants.Colours.YORK_RED;
            emb.Author.IconUrl = Constants.Images.ForkBot;
            emb.Footer.Text = "Remember, this list may not be accurate as courses may no longer be running or new courses may be added that are not on the list.";
            await ReplyAsync("", embed: emb.Build());


        }

        [Command("debugmode"), Summary("[BRADY] Set bot to debug mode, disables other users and enables some other features.")]
        public async Task DebugMode(int code)
        {
            if (code == Var.DebugCode)
            {
                Var.DebugMode = !Var.DebugMode;
                Console.WriteLine("DebugMode set to " + Var.DebugMode);
            }
        }
        /*
        [Command("snap")]
        public async Task Snap()
        {
            if (Context.User.Id != Constants.Users.BRADY) return;
            await Context.Channel.SendMessageAsync("When I’m done, half of humanity will still exist. Perfectly balanced, as all things should be.\n\nI know what it’s like to lose. To feel so desperately that you’re right, yet to fail nonetheless. Dread it. Run from it. Destiny still arrives. Or should I say, I have.");
            
            var users = await Context.Guild.GetUsersAsync();
            List<int> dustIndex = new List<int>();
            int userCount = users.Count();
            int halfUsers = userCount / 2;
            for (int i = 0; i < halfUsers; i++)
            {
                try
                {
                    int index = -1;
                    while (index < 0 || dustIndex.Contains(index))
                        index = rdm.Next(userCount);

                    dustIndex.Add(index);
                    await users.ElementAt(index).AddRoleAsync(Context.Guild.GetRole(Constants.Roles.DUST));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.StackTrace}\n\n on user index {i} ({users.ElementAt(i).Username})");
                }
            }
        }
        */


        #endregion

    }
    
}