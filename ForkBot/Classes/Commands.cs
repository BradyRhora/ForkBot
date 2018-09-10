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
            var node = page.DocumentNode.SelectSingleNode("//*[@id=\"searchResultsBox\"]/div[2]/ul/li[1]");
            if (node != null)
            {
                string tid = Functions.GetTID(node.InnerHtml);

                var newLink = "http://www.ratemyprofessors.com/ShowRatings.jsp?tid=" + tid;
                page = web.Load(newLink);

                var rating = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[1]/div/div/div").InnerText;
                var takeAgain = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[2]/div[1]/div").InnerText;
                var difficulty = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[1]/div/div[2]/div[2]/div").InnerText;
                var imageNode = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[1]/div[2]/div[1]/div[1]/img");
                var titleText = page.DocumentNode.SelectSingleNode("/html/head/title").InnerText;
                string profName = titleText.Split(' ')[0] + " " + titleText.Split(' ')[1];
                string university = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[1]/div[2]/div[1]/div[3]/h2/a").InnerText;
                university = university.Replace(" (all campuses)", "");
                var tagsNode = page.DocumentNode.SelectSingleNode("//*[@id=\"mainContent\"]/div[1]/div[3]/div[2]/div[2]");
                List<string> tags = new List<string>();
                for (int i = 0; i < tagsNode.ChildNodes.Count(); i++)
                {
                    if (tagsNode.ChildNodes[i].Name == "span") tags.Add(tagsNode.ChildNodes[i].InnerText);
                }
                string imageURL = null;
                if (imageNode != null) imageURL = imageNode.Attributes[0].Value;

                var commentsNode = page.DocumentNode.SelectSingleNode("/ html[1] / body[1] / div[2] / div[4] / div[3] / div[1] / div[7] / table[1]");

                List<string> comments = new List<string>();
                for (int i = 3; i < commentsNode.ChildNodes.Count(); i++)
                {
                    if (commentsNode.ChildNodes[i].Name == "tr" && commentsNode.ChildNodes[i].Attributes.Count() == 2)
                    {
                        comments.Add(commentsNode.ChildNodes[i].ChildNodes[5].ChildNodes[3].InnerText.Replace("\r\n               ", "").Replace("/", " "));
                    }
                }
                List<string> words = new List<string>();
                List<int> counts = new List<int>();

                foreach (string comment in comments)
                {
                    foreach (string dWord in comment.Split(' '))
                    {
                        string word = dWord.ToLower().Replace(".", "").Replace(",", "").Replace("'", "").Replace("(", "").Replace(")", "").Replace("!", "").Replace("?", "");
                        if (word != "")
                        {
                            if (words.Contains(word)) counts[words.IndexOf(word)]++;
                            else
                            {
                                words.Add(word);
                                counts.Add(1);
                            }
                        }
                    }
                }

                List<string> OrderedWords = new List<string>();
                for (int i = counts.Max(); i >= 0; i--)
                {
                    for (int c = 0; c < counts.Count(); c++)
                    {
                        if (counts[c] == i)
                        {
                            OrderedWords.Add(words[counts.IndexOf(counts[c])]);
                            break;
                        }
                    }
                }
                string[] commonWords = { "i", "me", "at", "youll", "if", "an", "not", "it", "as", "is", "in", "for", "but", "so", "on", "he", "the", "and", "to", "a", "are", "his", "she", "her", "you", "of", "hes", "shes", "prof", profName.ToLower().Split(' ')[0], profName.ToLower().Split(' ')[1], "we" };
                foreach (string wrd in commonWords) OrderedWords.Remove(wrd);

                JEmbed emb = new JEmbed();

                emb.Title = profName + " - " + university;
                if (imageURL != null) emb.ImageUrl = imageURL;
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

                emb.Fields.Add(new JEmbedField(x =>
                {
                    x.Header = "Common Comments:";
                    string text = "";
                    foreach (string s in OrderedWords)
                    {
                        text += Func.ToTitleCase(s) + ", ";
                    }
                    text = text.Substring(0, text.Count() - 2);
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
                if (Regex.IsMatch(code, "([A-z]{2,4} *[0-9]{4})"))
                {
                    var splits = Regex.Split(code, "(\\d+|\\D+)").Where(x=>x!="").ToArray();
                    code = splits[0].Trim() + " " + splits[1].Trim();
                }

                HtmlWeb web = new HtmlWeb();
                string desc, title, link = "";

                var courses = File.ReadAllLines("Files/courselist.txt");
                foreach (string course in courses)
                {
                    if (course.ToLower().Contains(code.ToLower()))
                    {
                        var info = course.Split(' ');
                        var department = info[0].Split('/')[0];
                        var subject = info[0].Split('/')[1];
                        var coursecode = info[1];
                        var credit = info[2].Split('\t')[0];
                        link = $"https://w2prod.sis.yorku.ca/Apps/WebObjects/cdm.woa/wa/crsq?fa={department}&sj={subject}&cn={coursecode}&cr={credit}&ay=2018&ss=FW";
                    }
                }

                if (link == "") throw new Exception("Unable to find course.");
                var pageDoc = web.Load(link).DocumentNode;

                desc = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]").ChildNodes[5].InnerText;
                title = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/table[1]/tr[1]/td[1]").InnerText.Replace("&nbsp;", "");
                desc = desc.Replace("&quot;", "\"");
                var scheduleNode = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/p[7]/a[1]");
                var scheduleLink = "https://w2prod.sis.yorku.ca" + scheduleNode.Attributes[0].Value;


                JEmbed emb = new JEmbed();
                emb.Title = title;
                emb.Description = desc + "\n\n";
                emb.ColorStripe = Constants.Colours.YORK_RED;

                pageDoc = web.Load(scheduleLink).DocumentNode;
                var table = pageDoc.SelectSingleNode("/html[1]/body[1]/table[1]/tr[2]/td[2]/table[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]/table[2]");
                foreach (HtmlNode child in table.ChildNodes)
                {
                    if (child.Name == "tr")
                    {
                        var termSec = child.SelectSingleNode($"td/table/tr[1]").InnerText.Replace("\n", "").Replace("\t", "").Replace("&nbsp;", "").Trim();
                        var sessionDir = child.SelectSingleNode($"td/table/tr[2]").InnerText.Replace("Please click here to see availability.", "").Replace("&nbsp;", "").Trim();
                        var schedule = child.SelectSingleNode($"td/table/tr[3]/td/table/tr[2]");
                        var type = schedule.ChildNodes[0].InnerText;
                        var timedayInfo = schedule.ChildNodes[1].InnerText.Replace("&nbsp;", "").Trim().Replace("     ", "|").Replace(" ", "").Split('|');

                        emb.Description += $"\n{termSec} - {sessionDir}";

                        for (int i = 0; i < timedayInfo.Count(); i++)
                        {
                            string day = Convert.ToString(timedayInfo[i][0]);
                            string time = "";
                            for (int o = 2; o < timedayInfo[i].Length; o++)
                            {
                                if (timedayInfo[i][o - 2] == ':') time = timedayInfo[i].Substring(1, o);
                            }
                            emb.Description += $"\n\t\t{day} - {time}";
                        }

                    }
                }

                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            catch (Exception) { await Context.Channel.SendMessageAsync("There was an error loading the course page. (Possibly not available this term)"); }

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
            await Context.Channel.SendMessageAsync("```\nFORKBOT CHANGELOG 1.83\n-added ;top bottom and ;slots\n-fixed shop bug```");
        }

        #endregion

        #region Item Commands
        [Command("sell"), Summary("[FUN] Sell items from your inventory.")]
        public async Task Sell(params string[] items)
        {
            var u = Functions.GetUser(Context.User);
            string msg = "";
            var itemList = Functions.GetItemList();
            var rItems = Functions.GetRareItemList();
            foreach (string item in items)
            {
                if (u.GetItemList().Contains(item))
                {
                    u.RemoveItem(item);
                    int price = 0;
                    bool unsold = false;
                    foreach (string line in itemList)
                    {
                        if (line.Split('|')[0] == item)
                        {
                            if (line.Split('|')[2] != "-")
                                price = (int)(Convert.ToInt32(line.Split('|')[2]) * .75);
                            else unsold = true;
                            break;
                        }
                    }

                    /*if (rdm.Next(100) < 5 && !unsold)
                    {
                        var rItemData = rItems[rdm.Next(rItems.Count())];
                        var itemName = rItemData.Split('|')[0].Replace('_', ' ');
                        var rMessage = rItemData.Split('|')[1];
                        u.GiveCoins(-price);
                        u.GiveItem(item.Replace(' ','_'));
                        msg += $"Wait... Something is happening.... Your {Func.ToTitleCase(item)} floats up into the air and glows... It becomes.. My GOD... IT BECOMES....\n\n" +
                                                               $"A {itemName}! {Functions.GetItemEmote(rItemData)} {rMessage}\n";
                    }
                    else */
                    if (!unsold)
                    {
                        u.GiveCoins(price);
                        msg += $"You successfully sold your {item} for {price} coins!\n";
                    }
                    else
                    {
                        u.GiveItem(item);
                        msg += "This item cannot be sold. Have you tried using it's command?";
                    }
                }
                else msg += $"You do not have an item called {item}!";
            }

            await Context.Channel.SendMessageAsync(msg);
        }

        [Command("trade"), Summary("[FUN] Initiate a trade with another user!")]
        public async Task Trade(IUser user)
        {
            if (Functions.GetTrade(Context.User) == null && Functions.GetTrade(user) == null)
            {
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
                        if (!trade.Accepted) trade.Accept();
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
                                await Context.Channel.SendMessageAsync("Unable to add item. Are you sure you have it?");
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
                        await Context.Channel.SendMessageAsync("", embed: new InfoEmbed("TRADE CANCELLED", $"{Context.User.Username} has cancelled the trade. All items have been returned.").Build());
                        Var.trades.Remove(trade);
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

        [Command("shop"), Summary("[FUN] Open the shop and buy stuff! New items each day.")]
        public async Task Shop([Remainder] string command = null)
        {
            var u = Functions.GetUser(Context.User);
            DateTime day = new DateTime();
            DateTime currentDay = new DateTime();
            if (Var.currentShop != null)
            {
                day = Var.currentShop.Date();
                currentDay = DateTime.UtcNow - new TimeSpan(5, 0, 0);
            }
            if (Var.currentShop == null || day.DayOfYear < currentDay.DayOfYear && day.Year == currentDay.Year)
            {
                Var.currentShop = new Shop();
            }

            List<string> itemNames = new List<string>();
            foreach (string item in Var.currentShop.Items()) itemNames.Add(item.Split('|')[0]);



            if (command == null)
            {
                JEmbed emb = new JEmbed();
                emb.Title = "Shop";
                emb.ThumbnailUrl = Constants.Images.ForkBot;
                emb.ColorStripe = Constants.Colours.YORK_RED;
                foreach (string item in Var.currentShop.Items())
                {
                    var data = item.Split('|');
                    string emote = Functions.GetItemEmote(item);
                    string name = data[0];
                    string desc = data[1];
                    int price = Convert.ToInt32(data[2]);
                    if (price < 0) price = -price;
                    emb.Fields.Add(new JEmbedField(x =>
                    {
                        x.Header = $"{emote} {name.Replace("_", " ")} - {price} coins";
                        x.Text = desc;
                    }));
                }
                emb.Footer.Text = $"You have: {u.GetCoins()} coins.";
                await Context.Channel.SendMessageAsync("", embed: emb.Build());
            }
            else if (itemNames.Contains(command.ToLower().Replace(" ", "_")))
            {
                foreach (string item in Var.currentShop.Items())
                {
                    if (item.Split('|')[0] == command.ToLower().Replace(" ", "_"))
                    {
                        var data = item.Split('|');
                        string name = data[0];
                        string desc = data[1];
                        int price = Convert.ToInt32(data[2]);
                        if (price < 0) price *= -1;

                        if (Convert.ToInt32(u.GetCoins()) >= price)
                        {
                            u.GiveCoins(-price);
                            u.GiveItem(name);
                            await Context.Channel.SendMessageAsync($":shopping_cart: You have successfully purchased a(n) {name} {Functions.GetItemEmote(Functions.GetItemData(name))} for {price} coins!");
                        }
                        else await Context.Channel.SendMessageAsync("You cannot afford this item.");
                    }
                }
            }
            else await Context.Channel.SendMessageAsync("Either something went wrong, or this item isn't in stock!");
        }

        [Command("freemarket"), Alias("fm", "market"), Summary("[FUN] Sell items to other users! Choose your own price!")]
        public async Task FreeMarket(params string[] command)
        {
            throw new NotImplementedException("Not ready for public.");


            if (command.Count() == 0) await ReplyAsync("Use one of the following commands!\n```\n;fm view\n;fm sell [item] [price]\n;fm buy [item_id]\n```");
            else if (command[0] == "view")
            {
                FileStream fs = new FileStream("Files/MarketItems.xml", FileMode.Open);
                List<string> itemList = new List<string>();
                using (XmlReader reader = XmlReader.Create(fs))
                {
                    while (reader.Read())
                    {
                        if (reader.Name == "market") reader.Read();
                        if (reader.NodeType != XmlNodeType.Whitespace && reader.NodeType != XmlNodeType.None)
                        {
                            string id = reader.GetAttribute("id");
                            string cost = reader.GetAttribute("cost");
                            var user = Bot.client.GetUser(Convert.ToUInt64(reader.GetAttribute("seller")));
                            string itemName = reader.GetAttribute("name");
                            string emote = Functions.GetItemEmote(Functions.GetItemData(itemName));
                            itemList.Add($"{emote} {itemName} - {cost} coins ~ {user.Username}|[{id}] ");
                        }
                    }
                }

                JEmbed emb = new JEmbed();
                emb.Author.Name = "Free Market";

                foreach (string item in itemList)
                {
                    emb.Fields.Add(new JEmbedField(x =>
                    {
                        x.Header = item.Split('|')[0];
                        x.Text = item.Split('|')[1];
                    }));
                }

                emb.Footer.Text = "Use \';fm sell [item] [price]\' or \';fm buy [item_id]\'";

                await ReplyAsync("", embed: emb.Build());

            }
            else if (command[0] == "sell")
            {
                string item = command[1];
                string price = command[2];
                User user = Functions.GetUser(Context.User);
                if (Functions.CheckUserHasItem(user, item))
                {
                    await ReplyAsync("You do not have an item called: " + item + ".");
                    return;
                }

                XmlDocument doc = new XmlDocument();
                using (FileStream fs = new FileStream("Files/MarketItems.xml", FileMode.Open))
                {
                    doc.Load(fs);
                    bool idSet = false;
                    string id = "";
                    char asciiA = 'A';
                    char asciiZ = 'Z';
                    do
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            id += rdm.Next(10);
                            id += (char)rdm.Next(asciiA, asciiZ + 1);
                        }

                        foreach (XmlElement element in doc.GetElementsByTagName("item"))
                        {
                            if (element.GetAttribute("id") != id) idSet = true;
                        }
                    } while (!idSet);

                    var lastChild = doc.LastChild;
                    XmlElement elem = doc.CreateElement("item");
                    elem.SetAttribute("name", item);
                    elem.SetAttribute("id", id);
                    elem.SetAttribute("cost", price);
                    elem.SetAttribute("seller", Convert.ToString(Context.User.Id));
                    doc.FirstChild.AppendChild(elem);
                    doc.Save("Files/MarketItems.xml");

                    await ReplyAsync($"{item} successfully added to Free Market at {price} coin(s) with ID: {id}");
                }
            }
            else if (command[0] == "buy")
            {
                string id = command[1];
                XmlDocument document = new XmlDocument();

                using (FileStream fs = new FileStream("Files/MarketItems.xml", FileMode.Open))
                {
                    document.Load(fs);
                    foreach (XmlElement element in document.FirstChild)
                    {
                        if (element.HasAttributes)
                        {
                            if (element.GetAttribute("id") == id)
                            {
                                var user = Functions.GetUser(Context.User);
                                int userCoins = user.GetCoins();
                                int price = Convert.ToInt32(element.GetAttribute("cost"));
                                if (userCoins >= price)
                                {
                                    user.GiveCoins(-price);
                                    user.GiveItem(element.GetAttribute("name"));
                                    var sellerUser = Bot.client.GetUser(Convert.ToUInt64(element.GetAttribute("seller")));
                                    var seller = Functions.GetUser(sellerUser);
                                    await sellerUser.SendMessageAsync($"{Context.User.Username} has purchased your {element.GetAttribute("name")} for {price} coins!");
                                    seller.GiveCoins(price);
                                    await ReplyAsync("Successfully purchased item.");
                                    document.FirstChild.RemoveChild(element);
                                    document.Save(fs);
                                    fs.Close();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
        
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
        
        #region Fun

        [Command("draw"), Summary("[FUN] Gets ForkBot to draw you a lovely picture")]
        public async Task Draw(int count)
        {
            if (count > 99999) count = 99999;
            int size = 500;
            using (Bitmap bmp = new Bitmap(size, size))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    int x = rdm.Next(size);
                    int y = rdm.Next(size);
                    var c = System.Drawing.Color.FromArgb(rdm.Next(256), rdm.Next(256), rdm.Next(256));

                    for (int i = 0; i < count; i++)
                    {
                        Brush b = new SolidBrush(c);
                        g.FillEllipse(b, x, y, 10, 10);
                        int mult = 0;
                        mult = rdm.Next(-1, 2);
                        x += 5 * mult;
                        mult = rdm.Next(-1, 2);
                        y += 5 * mult;
                        if (rdm.Next(100) == 1 || x > size || x < 0 || y > size || y < 0)
                        {
                            x = rdm.Next(size);
                            y = rdm.Next(size);
                            c = System.Drawing.Color.FromArgb(rdm.Next(256), rdm.Next(256), rdm.Next(256));
                        }
                    }

                    bmp.Save("Files/drawing.png");
                    await Context.Channel.SendFileAsync("Files/drawing.png");

                }
            }
        }

        [Command("choose"), Summary("[FUN] Get ForkBot to make your decisions for you! Seperate choices with `|`")]
        public async Task Choose([Remainder] string input)
        {
            var choices = input.Split('|');

            var decision = choices[rdm.Next(choices.Count())];

            await Context.Channel.SendMessageAsync($"{Context.User.Username}, I choose...");
            await Context.Channel.SendMessageAsync(decision + "!");

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
                        await Context.Channel.SendMessageAsync("You did it!");
                        Var.hangman = false;
                        foreach (char c in Var.hmWord)
                        {
                            Var.guessedChars.Add(c);
                        }
                        var u = Functions.GetUser(Context.User);
                        u.GiveCoins(10);
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
        public async Task Profile(IUser user)
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

            emb.Fields.Add(new JEmbedField(x =>
            {
                x.Header = "Roles:";
                string text = "";
                foreach (ulong id in (user as IGuildUser).RoleIds)
                {
                    text += Context.Guild.GetRole(id).Name + "\n";
                }

                x.Text = Convert.ToString(text);
                x.Inline = true;
            }));

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
                if (Var.presentTime < DateTime.Now - Var.presentWait)
                {
                    Var.presentCount = rdm.Next(4) + 1;
                    Var.presentClaims.Clear();
                }

                if (Var.presentCount > 0 && !Var.presentClaims.Any(x => x.Id == Context.User.Id))
                {
                    if (Var.presentClaims.Count() <= 0)
                    {
                        Var.presentWait = new TimeSpan(rdm.Next(4), rdm.Next(60), rdm.Next(60));
                        Var.presentTime = DateTime.Now;
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
                        int lossCount = rdm.Next(5) + 1;
                        if (lossCount > user.GetItemList().Count()) lossCount = user.GetItemList().Count();
                        if (lossCount == 0)
                        {
                            await ReplyAsync($":bomb: Oh no! The present was rigged by {Var.presentRigger.Mention} and you lost... Nothing??\n:boom::boom::boom::boom:");
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
                else
                {
                    var timeLeft = Var.presentTime - (DateTime.Now - Var.presentWait);
                    var msg = $"The next presents are not available yet! Please be patient! They should be ready in *about* {timeLeft.Hours + 1} hour(s)!\nThere are {Var.presentCount} presents left!";
                    if (Var.presentClaims.Count() > 0)
                    {
                        msg += "\nLast claimed by:\n```\n";
                        foreach (IGuildUser user in Var.presentClaims) msg += $"\n{user.Username} in {user.Guild}";
                        msg += "\n```";
                    }
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
            if (stat == "bottom") stat = "";
            string msg = "```\nTop five users";
            if (stat != "") msg += " [" + stat + "]:\n";
            else msg += ":\n";
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

        [Command("forkopoly"), Summary("[FUN] Play a game of Forkopoly™ with your best pals!"), Alias(new string[]{"fp"})]
        public async Task Forkopoly(string command)
        {
            
        }

        #endregion

        #region Mod Commands

        [Command("ban"),RequireUserPermission(GuildPermission.BanMembers), Summary("[MOD] Bans the specified user. Can enter time in minutes that user is banned for, otherwise it is indefinite.")]
        public async Task Ban(IGuildUser u, int minutes = 0, [Remainder]string reason = null)
        {
            string rText = ".";
            if (reason != null) rText = $" for: \"{reason}\".";

            string tText = "";

            if (minutes != 0)
            {
                TimeSpan tSpan = new TimeSpan(0, minutes, 0);
                var unbanTime = DateTime.Now + tSpan;
                Var.leaveBanned.Add(u);
                Var.unbanTime.Add(unbanTime);
                tText = $"\nThey have been banned until {unbanTime}.";
            }

            InfoEmbed banEmb = new InfoEmbed("USER BAN", $"User: {u} has been banned{rText}.{tText}", Constants.Images.Ban);
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

        [Command("move"), RequireUserPermission(GuildPermission.KickMembers), Summary("[MOD] Move people who are in the wrong channel to the correct channel.")]
        public async Task Move(IMessageChannel chan, params IUser[] users)
        {
            string mentionedUsers = "";
            foreach(IUser u in users)
            {
                mentionedUsers += u.Mention + ", ";
            }
            await chan.SendMessageAsync(mentionedUsers + " please keep discussions in their correct channel.");
            var channels = await Context.Guild.GetTextChannelsAsync();
            OverwritePermissions op = new OverwritePermissions(readMessages: PermValue.Deny);
            
            foreach (IGuildChannel c in channels)
            {
                foreach (IUser u in users)
                {
                    try
                    {
                        if (c != null && c.Id != chan.Id) await c.AddPermissionOverwriteAsync(u, op);
                    }
                    catch (Exception ) { }
                }
            }
            Timers.mvChannel = chan;
            Timers.mvChannels = channels;
            Timers.mvUsers = users;

            Timers.mvTimer = new Timer(Timers.MoveTimer, null, 5000, Timeout.Infinite);
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

        [Command("block"), RequireUserPermission(GuildPermission.KickMembers), Summary("[MOD] Temporarily stops users from being able to use the bot.")]
        public async Task Block(IUser u)
        {
            if (Var.blockedUsers.Contains(u)) Var.blockedUsers.Remove(u);
            else Var.blockedUsers.Add(u);
            
        }
        
        [Command("blockword"), RequireUserPermission(GuildPermission.ManageMessages), Summary("[MOD] Adds the inputted word to the word filter.")]
        public async Task BlockWord([Remainder] string word)
        {
            Properties.Settings.Default.blockedWords += word + "|";
            Properties.Settings.Default.Save();
            await ReplyAsync("", embed: new InfoEmbed("Word Blocked", "Word successfully added to filter.").Build());
            await Context.Message.DeleteAsync();
        }

        #endregion

        #region Brady Commands

        [Command("remind"), Summary("[BRADY] View, add, and remove reminders.")]
        public async Task Remind([Remainder] string reminder = "")
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




        

        #endregion

    }
    
}